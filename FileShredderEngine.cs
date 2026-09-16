using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureShredder
{
    public static class FileShredderEngine
    {
        private const int BufferSize = 64 * 1024; // 64 KB buffer
        private const int TotalPasses = 3;

        /// <summary>
        /// Overwrites file sectors with multi-pass pattern data, flushes disk cache, 
        /// renames metadata entries, and deletes the file.
        /// </summary>
        public static async Task ShredFileAsync(
            string filePath,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Target file for shredding does not exist.", filePath);
            }

            // Remove ReadOnly or System attributes if present
            File.SetAttributes(filePath, FileAttributes.Normal);

            var fileInfo = new FileInfo(filePath);
            long fileLength = fileInfo.Length;

            // Handle 0-byte edge case
            if (fileLength == 0)
            {
                ObfuscateAndDelete(filePath);
                progress?.Report(100);
                return;
            }

            using (var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                useAsync: true))
            {
                byte[] buffer = new byte[BufferSize];

                for (int pass = 1; pass <= TotalPasses; pass++)
                {
                    stream.Position = 0;
                    long bytesWritten = 0;

                    while (bytesWritten < fileLength)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        int currentChunk = (int)Math.Min(BufferSize, fileLength - bytesWritten);

                        // Select overwrite pattern per pass
                        switch (pass)
                        {
                            case 1: // Pass 1: Cryptographic Random Data
                                RandomNumberGenerator.Fill(buffer.AsSpan(0, currentChunk));
                                break;
                            case 2: // Pass 2: Zeros (0x00)
                                Array.Clear(buffer, 0, currentChunk);
                                break;
                            case 3: // Pass 3: Cryptographic Random Data
                                RandomNumberGenerator.Fill(buffer.AsSpan(0, currentChunk));
                                break;
                        }

                        await stream.WriteAsync(buffer.AsMemory(0, currentChunk), cancellationToken);
                        bytesWritten += currentChunk;

                        // Calculate total overall percentage across all 3 passes
                        double passProgress = (double)bytesWritten / fileLength;
                        double overallPercentage = ((pass - 1) + passProgress) / TotalPasses * 100;
                        progress?.Report(overallPercentage);
                    }

                    // Flush OS memory buffers directly to physical disk media
                    stream.Flush(flushToDisk: true);
                }

                // Truncate file size to 0 bytes
                stream.SetLength(0);
                stream.Flush(flushToDisk: true);
            }

            // Obfuscate directory record entry and delete
            ObfuscateAndDelete(filePath);
        }

        private static void ObfuscateAndDelete(string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory)) return;

            // Rename file to a random name (e.g. "a8f3b21c.tmp") to scramble original metadata
            string randomName = Path.Combine(directory, Path.GetRandomFileName());
            File.Move(filePath, randomName);

            // Final file system deletion
            File.Delete(randomName);
        }
    }
}
