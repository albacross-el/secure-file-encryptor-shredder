# Secure File Encryptor Shredder

A secure file encryption and shredding utility for protecting sensitive data. This Windows application provides a straightforward way to encrypt confidential files and securely shred sensitive data to prevent recovery.

## Features

- **File Encryption** - Protect files with strong cryptographic algorithms
- **Secure Shredding** - Permanently erase files to prevent data recovery
- **Fast Processing** - Efficient handling of large files
- **Command-Line & GUI** - Flexible interfaces for automation and ease of use
- **Batch Operations** - Process multiple files in one operation

## Getting Started

### Requirements

- Windows OS (.NET Framework or .NET Core runtime)
- Administrator privileges (recommended for secure operations)

### Installation

1. Clone or download the repository
2. Build the project using Visual Studio or the .NET CLI:
   ```bash
   dotnet build
   ```
3. The compiled `.exe` file will be located in the build output directory
4. (Optional) Add the executable directory to your PATH for easy command-line access

### Running the Executable

**Standalone Usage:**
- Locate `SecureFileEncryptorShredder.exe` in the build output folder
- Double-click to run the GUI application, or
- Run from command line for automated operations

**Command-Line Usage:**
```bash
SecureFileEncryptorShredder.exe --encrypt "path/to/file"
SecureFileEncryptorShredder.exe --shred "path/to/file"
```

## Usage

### Encrypting Files
1. Select the file you want to encrypt
2. Choose a strong password
3. The encrypted file will be saved with `.encrypted` extension

### Shredding Files
1. Select the file you want to securely delete
2. Confirm the operation
3. The file will be overwritten multiple times and removed

### Batch Operations
Process multiple files at once by specifying a directory path or multiple file paths.

## How It Works

- **Encryption**: Files are encrypted using industry-standard algorithms (AES-256)
- **Shredding**: Data is overwritten multiple times before deletion to ensure recovery is impossible
- **Memory Safety**: Temporary data and encryption keys are securely cleared from memory after use

## Security Considerations

- Use strong, unique passwords for encrypted files
- Store passwords securely (consider using a password manager)
- Ensure you have administrative privileges for secure shredding
- Keep backups of important files before shredding
- Shredded files cannot be recovered once the operation completes

## License

See LICENSE file for details.

## Contributing

Contributions are welcome. Please follow standard practices for code submission and ensure all changes maintain security best practices.
