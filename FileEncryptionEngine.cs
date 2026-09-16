using System;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureShredder
{
    public static class FileEncryptionEngine
    {
        // Cryptographic Constants
        private static readonly byte[] MagicHeader = "SENC"u8.ToArray(); // File Signature (4 bytes)
        private const int KeySize = 32;       // 256-bit key
        private const int SaltSize = 16;      // 128-bit salt
        private const int NonceSize = 12;     // 96-bit base nonce for AES-GCM
        private const int TagSize = 16;       // 128-bit authentication tag
        private const int ChunkSize = 64 * 1024; // 64 KB chunk size
        private const int Pbkdf2Iterations = 600_000; // PBKDF2 iteration count

        /// <summary>
        /// Encrypts a file using AES-256-GCM streaming with PBKDF2 key derivation.
        /// </summary>
        public static async Task EncryptFileAsync(
            string inputPath,
            string outputPath,
            string password,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] baseNonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] key = DeriveKey(password, salt);

            try
            {
                using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, ChunkSize, useAsync: true);
                using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, ChunkSize, useAsync: true);
                using var aesGcm = new AesGcm(key, TagSize);

                // 1. Write Header: [ Magic (4B) | Salt (16B) | Base Nonce (12B) ]
                await outputStream.WriteAsync(MagicHeader, cancellationToken);
                await outputStream.WriteAsync(salt, cancellationToken);
                await outputStream.WriteAsync(baseNonce, cancellationToken);

                byte[] buffer = new byte[ChunkSize];
                byte[] ciphertext = new byte[ChunkSize];
                byte[] tag = new byte[TagSize];
                byte[] currentNonce = new byte[NonceSize];
                byte[] lengthBuffer = new byte[4];

                long totalBytes = inputStream.Length;
                long totalBytesRead = 0;
                uint chunkIndex = 0;

                int bytesRead;
                while ((bytesRead = await inputStream.ReadAsync(buffer.AsMemory(0, ChunkSize), cancellationToken)) > 0)
                {
                    // Derive chunk nonce by combining base nonce and chunk index counter
                    PrepareChunkNonce(baseNonce, currentNonce, chunkIndex++);

                    // Encrypt current chunk
                    aesGcm.Encrypt(
                        currentNonce,
                        buffer.AsSpan(0, bytesRead),
                        ciphertext.AsSpan(0, bytesRead),
                        tag);

                    // Write Chunk Header & Payload: [ Length (4B) | Tag (16B) | Encrypted Bytes ]
                    BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, bytesRead);
                    await outputStream.WriteAsync(lengthBuffer, cancellationToken);
                    await outputStream.WriteAsync(tag, cancellationToken);
                    await outputStream.WriteAsync(ciphertext.AsMemory(0, bytesRead), cancellationToken);

                    totalBytesRead += bytesRead;
                    progress?.Report((double)totalBytesRead / totalBytes * 100);
                }
            }
            finally
            {
                CryptographicOperations.ZeroMemory(key);
            }
        }

        /// <summary>
        /// Decrypts a file encrypted with EncryptFileAsync, verifying tag integrity.
        /// </summary>
        public static async Task DecryptFileAsync(
            string inputPath,
            string outputPath,
            string password,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, ChunkSize, useAsync: true);

            // 1. Validate File Signature
            byte[] headerBuffer = new byte[MagicHeader.Length];
            if (await inputStream.ReadAsync(headerBuffer, cancellationToken) < MagicHeader.Length ||
                !CryptographicOperations.FixedTimeEquals(headerBuffer, MagicHeader))
            {
                throw new InvalidDataException("Invalid file format or file is not encrypted with this application.");
            }

            // 2. Read Salt and Base Nonce
            byte[] salt = new byte[SaltSize];
            byte[] baseNonce = new byte[NonceSize];
            await inputStream.ReadExactlyAsync(salt, cancellationToken);
            await inputStream.ReadExactlyAsync(baseNonce, cancellationToken);

            byte[] key = DeriveKey(password, salt);

            try
            {
                using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, ChunkSize, useAsync: true);
                using var aesGcm = new AesGcm(key, TagSize);

                byte[] lengthBuffer = new byte[4];
                byte[] tag = new byte[TagSize];
                byte[] ciphertext = new byte[ChunkSize];
                byte[] plaintext = new byte[ChunkSize];
                byte[] currentNonce = new byte[NonceSize];

                long totalBytes = inputStream.Length;
                uint chunkIndex = 0;

                while (inputStream.Position < totalBytes)
                {
                    // Read Chunk Header: Length (4B) & Tag (16B)
                    await inputStream.ReadExactlyAsync(lengthBuffer, cancellationToken);
                    int payloadLength = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);

                    await inputStream.ReadExactlyAsync(tag, cancellationToken);
                    await inputStream.ReadExactlyAsync(ciphertext.AsMemory(0, payloadLength), cancellationToken);

                    // Derive chunk nonce
                    PrepareChunkNonce(baseNonce, currentNonce, chunkIndex++);

                    // Decrypt & Authenticate
                    aesGcm.Decrypt(
                        currentNonce,
                        ciphertext.AsSpan(0, payloadLength),
                        tag,
                        plaintext.AsSpan(0, payloadLength));

                    await outputStream.WriteAsync(plaintext.AsMemory(0, payloadLength), cancellationToken);

                    progress?.Report((double)inputStream.Position / totalBytes * 100);
                }
            }
            catch (AuthenticationTagMismatchException)
            {
                if (File.Exists(outputPath)) File.Delete(outputPath); // Clean up partial file
                throw new CryptographicException("Incorrect password or the file has been tampered with.");
            }
            finally
            {
                CryptographicOperations.ZeroMemory(key);
            }
        }

        private static byte[] DeriveKey(string password, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Pbkdf2Iterations,
                HashAlgorithmName.SHA256,
                KeySize);
        }

        private static void PrepareChunkNonce(byte[] baseNonce, byte[] targetNonce, uint chunkIndex)
        {
            baseNonce.CopyTo(targetNonce, 0);
            // XOR chunk counter into the last 4 bytes of the base nonce to guarantee unique IVs per chunk
            uint currentCounter = BinaryPrimitives.ReadUInt32BigEndian(targetNonce.AsSpan(8));
            BinaryPrimitives.WriteUInt32BigEndian(targetNonce.AsSpan(8), currentCounter ^ chunkIndex);
        }
    }
}
