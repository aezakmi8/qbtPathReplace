# qbtPathReplace

`qbtPathReplace` is a C# tool designed to batch update file paths in qBittorrent `.fastresume` files. It replaces an existing save path with a new one, supporting both Windows (`\` separators) and Linux (`/` separators) path formats. This is useful when migrating torrents between different systems or drives.

## Features
- Batch updates paths in `.fastresume` files located in the qBittorrent `BT_backup` directory.
- Supports both Windows and Linux path formats via a configurable flag.
- Preserves subdirectories relative to the existing path.
- Includes logging to track changes made to each file.

## Prerequisites
- **.NET Framework**: Ensure you have .NET Framework 4.8 or higher installed (or .NET Core/.NET 5+ if adapted).
- **BencodeNET**: This project uses the `BencodeNET` library to parse and modify `.fastresume` files.

## Installation
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/yourusername/qbtPathReplace.git
   cd qbtPathReplace

## Usage

Run the program from the command line with the following syntax:

### Arguments

- **`<btBackupPath>`**: The path to the qBittorrent `BT_backup` directory containing `.fastresume` files.  
  *Example*: `"C:\Users\Work\Downloads\qBittorrent\BT_backup"`

- **`<existingPath>`**: The current save path in the `.fastresume` files that you want to replace. Must match exactly, followed by a separator (`/` or `\`) or the end of the path.  
  *Example*: `"E:\Download"`

- **`<newPath>`**: The new save path to replace the existing one with.  
  *Example*: `"/downloads/ad_content"`

- **`<targetPathLinux>`**: A boolean value indicating the target path format:  
  - `true`: Use Linux-style separators (`/`).  
  - `false`: Use Windows-style separators (`\`).  
  *Example*: `true`

### Examples

1. **Replace a Windows path with a Linux path**:
   ```bash
   qbtPathReplace "C:\Users\Work\Downloads\qBittorrent\BT_backup" "E:\Download" "/downloads/ad_content" true
