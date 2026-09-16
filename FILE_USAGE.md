# FILE USAGE - Form1.cs

## File Summary
**Form1.cs** is the primary Windows Forms UI controller for the Secure File Encryptor and Shredder application. It serves as the main graphical interface through which users interact with file encryption, decryption, and secure deletion operations.

---

## Purpose & Responsibility

### Core Functions:
1. **Drag-and-Drop File Input** - Provides an intuitive drag-and-drop zone for file selection
2. **Password Input Management** - Handles secure password entry from the user
3. **Encryption Workflow** - Orchestrates the file encryption process with real-time progress feedback
4. **Decryption Workflow** - Manages decryption operations with error handling and password validation
5. **File Shredding Option** - Allows users to optionally destroy the original file after encryption
6. **Progress Tracking** - Displays live progress updates during encryption/decryption operations
7. **Error Handling & User Feedback** - Provides clear status messages and error dialogs

---

## Execution Flow

### **Initialization**
- Constructor initializes Windows Forms components
- Sets up drag-and-drop event listeners on the drop zone panel
- Disables encryption/decryption buttons until a file is selected

### **File Selection**
1. User drags a file into `pnlDropZone`
2. `DropZone_DragEnter()` validates it's a file (not text/data)
3. Visual feedback changes (blue highlight)
4. `DropZone_DragDrop()` accepts the file:
   - Stores file path in `_selectedFilePath`
   - Checks if file is encrypted (`.enc` extension)
   - Updates UI labels with filename
   - Enables encrypt/decrypt buttons
   - Auto-suggests next action (encrypt or decrypt)

### **Encryption Process** (`btnEncrypt_Click`)
1. Validates file exists and password is entered
2. Sets output filename as `originalFile.enc`
3. Disables UI controls to prevent interference
4. Shows progress bar
5. Calls `FileEncryptionEngine.EncryptFileAsync()`:
   - Uses AES-256-GCM encryption
   - Reports progress updates (0-100%)
6. **If Shred checkbox is checked:**
   - Calls `FileShredderEngine.ShredFileAsync()` on original file
   - Securely overwrites original data
   - Resets file selection UI
7. Displays success message with output file location
8. Re-enables UI controls

### **Decryption Process** (`btnDecrypt_Click`)
1. Validates encrypted file is selected
2. Validates password is entered
3. Determines output path:
   - Strips `.enc` extension if present
   - Otherwise appends `.dec`
4. Checks for file conflicts:
   - If output file exists, prompts user to overwrite
5. Disables UI controls
6. Calls `FileEncryptionEngine.DecryptFileAsync()`:
   - Validates password via GCM authentication tag
   - Reports progress updates
7. Handles three specific error cases:
   - **CryptographicException** - Wrong password or corrupted file
   - **IOException** - File locked or permission denied
   - **General Exception** - Other errors
8. Displays success/error message
9. Re-enables UI controls

### **Visual Feedback**
- **Drop Zone Colors:** Gray (default) → Light Blue (hover) → Gray (drop)
- **Status Label:** Updates with operation stage ("Processing: 45%", "Encrypting file...", etc.)
- **Progress Bar:** Shows real-time percentage completion
- **Button States:** Disabled until file is selected; disabled during operations

### **Control Management** (`ToggleControls`)
- Enables/disables all interactive elements based on operation state
- Ensures buttons are only active when a file is selected and not processing

---

## Security Considerations

✅ **Good Practices in This File:**
- No hardcoded passwords or keys
- Uses provided password only (not stored)
- Delegates actual crypto operations to `FileEncryptionEngine`
- Provides secure shredding option
- Validates user input before operations
- Handles cryptographic exceptions specifically

⚠️ **Notes:**
- Password entered in `txtPassword` text field is visible (consider masking with `PasswordChar = '*'`)
- Password remains in memory during operations (acceptable for UI layer)
- No logging of sensitive operations (good for security)

---

## Dependencies
- `FileEncryptionEngine` - Handles AES-256-GCM encryption/decryption
- `FileShredderEngine` - Handles secure file deletion
- System.Security.Cryptography - Crypto exception handling
- System.Windows.Forms - UI framework
