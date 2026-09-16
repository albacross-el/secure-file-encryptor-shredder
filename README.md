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

- Windows OS

### Installing the Executable

**Standalone Usage:**
- Locate `Secure Shredder.exe` in the downloads folder
- Double-click to run the installer application

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
