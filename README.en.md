# Windows MCP.Net

**English** | [中文](README.md)

A .NET-based Windows desktop automation MCP (Model Context Protocol) server that provides AI assistants with the ability to interact with the Windows desktop environment.

## 🚀 Features

### Core Functionality
- **Application Launch**: Launch applications from the Start Menu by name
- **PowerShell Integration**: Execute PowerShell commands and return results
- **Desktop State Capture**: Capture current desktop state including active applications, UI elements, etc.
- **Clipboard Operations**: Copy and paste text content
- **Mouse Operations**: Click, drag, move mouse cursor
- **Keyboard Operations**: Text input, key presses, keyboard shortcuts
- **Window Management**: Resize windows, adjust positions, switch applications
- **Scroll Operations**: Scroll at specified coordinates
- **Web Scraping**: Fetch web content and convert to Markdown format
- **Screenshot Functionality**: Capture screen and save to temporary directory
- **File System Operations**: Create, read, write, copy, move, delete files and directories
- **OCR Text Recognition**: Extract text from screen or specified regions, find text locations
- **Wait Control**: Add delays between operations

### Supported Tools

## Desktop Operation Tools

| Tool Name | Description |
|-----------|-------------|
| **LaunchTool** | Launch applications from the Start Menu |
| **PowershellTool** | Execute PowerShell commands and return status codes |
| **StateTool** | Capture desktop state information including applications and UI elements |
| **ClipboardTool** | Clipboard copy and paste operations |
| **ClickTool** | Mouse click operations (supports left, right, middle buttons, single, double, triple clicks) |
| **TypeTool** | Input text at specified coordinates with clear and enter support |
| **ResizeTool** | Resize window size and position |
| **SwitchTool** | Switch to specified application window |
| **ScrollTool** | Scroll at specified coordinates or current mouse position |
| **DragTool** | Drag from source coordinates to target coordinates |
| **MoveTool** | Move mouse cursor to specified coordinates |
| **ShortcutTool** | Execute keyboard shortcut combinations |
| **KeyTool** | Press individual keyboard keys |
| **WaitTool** | Pause execution for specified seconds |
| **ScrapeTool** | Scrape web content and convert to Markdown format |
| **ScreenshotTool** | Capture screen and save to temporary directory, return image path |

## FileSystem Tools

| Tool Name | Description |
|-----------|-------------|
| **ReadFileTool** | Read content from specified file |
| **WriteFileTool** | Write content to file |
| **CreateFileTool** | Create new file with specified content |
| **CopyFileTool** | Copy file to specified location |
| **MoveFileTool** | Move or rename file |
| **DeleteFileTool** | Delete specified file |
| **GetFileInfoTool** | Get file information (size, creation time, etc.) |
| **ListDirectoryTool** | List files and subdirectories in directory |
| **CreateDirectoryTool** | Create new directory |
| **DeleteDirectoryTool** | Delete directory and its contents |
| **SearchFilesTool** | Search for files in specified directory |

## OCR Image Recognition Tools

| Tool Name | Description |
|-----------|-------------|
| **ExtractTextFromScreenTool** | Extract text from entire screen using OCR |
| **ExtractTextFromRegionTool** | Extract text from specified screen region using OCR |
| **FindTextOnScreenTool** | Find specified text on screen using OCR |
| **GetTextCoordinatesTool** | Get coordinates of text on screen |

## 📸 Demo Screenshots

### Text Input Demo
Automatic text input in Notepad using TypeTool:

![Text Input Demo](assets/NotepadWriting.png)

### Web Search Demo
Open and search web content using ScrapeTool:

![Web Search Demo](assets/OpenWebSearch.png)

### 📹 Demo Video
Complete desktop automation operation demo:

[网页搜索演示](assets/video.mp4)

## 🛠️ Tech Stack

- **.NET 10.0**: Based on the latest .NET framework
- **Model Context Protocol**: Uses MCP protocol for communication
- **Microsoft.Extensions.Hosting**: Application hosting framework
- **Serilog**: Structured logging
- **HtmlAgilityPack**: HTML parsing and web scraping
- **ReverseMarkdown**: HTML to Markdown conversion

## 📦 Installation

### Prerequisites
- Windows Operating System
- .NET 10.0 Runtime or higher

**Important Note**: This project requires .NET 10 to run. Please ensure you have .NET 10 installed locally. If not installed, please visit the [.NET 10 Download Page](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) to download and install.

### Build from Source

```bash
# Clone repository
git clone https://github.com/AIDotNet/Windows-MCP.Net.git
cd Windows-MCP.Net/src

# Build project
dotnet build

# Run project
dotnet run
```

### NuGet Package Installation

```bash
dotnet tool install --global WindowsMCP.Net
```

## 🚀 Usage

### Run as MCP Server

```bash
# Direct run
dotnet run --project src/Windows-MCP.Net.csproj

# Or use installed tool
windows-mcp-net
```

### MCP Client Configuration

Add the following configuration to your MCP client:

#### Using Global Installed Tool (Recommended)
```json
{
    "mcpServers": {
     "WindowsMCP.Net": {
      "type": "stdio",
      "command": "dnx",
      "args": ["WindowsMCP.Net@", "--yes"],
      "env": {}
    }
    }
}
```

#### Using Project Source Code Direct Run (Development Mode)

**Method 1: Workspace Configuration**

Create `.vscode/mcp.json` file in project root:
```json
{
  "mcpServers": {
    "WindowsMCP.Net-Dev": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "src/Windows-MCP.Net.csproj"],
      "cwd": "${workspaceFolder}",
      "env": {}
    }
  }
}
```

**Method 2: User Configuration**

Run `MCP: Open User Configuration` through VS Code command palette, add:
```json
{
  "mcpServers": {
    "WindowsMCP.Net-Local": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "src/Windows-MCP.Net.csproj"],
      "env": {}
    }
  }
}
```

> **Note**: Using project source code method is convenient for development and debugging. Changes take effect without reinstallation. VS Code 1.102+ supports automatic discovery and management of MCP servers.


## 📖 API Documentation

### Example Operations

#### Launch Application
```json
{
  "tool": "LaunchTool",
  "parameters": {
    "name": "notepad"
  }
}
```

#### Click Operation
```json
{
  "tool": "ClickTool",
  "parameters": {
    "x": 100,
    "y": 200,
    "button": "left",
    "clicks": 1
  }
}
```

#### Text Input
```json
{
  "tool": "TypeTool",
  "parameters": {
    "x": 100,
    "y": 200,
    "text": "Hello, World!",
    "clear": false,
    "pressEnter": false
  }
}
```

#### Get Desktop State
```json
{
  "tool": "StateTool",
  "parameters": {
    "useVision": false
  }
}
```

#### Take Screenshot
```json
{
  "tool": "ScreenshotTool",
  "parameters": {}
}
```

## 🏗️ Project Structure

```
src/
├── Windows-MCP.Net/         # Main project
│   ├── .mcp/                # MCP server configuration
│   │   └── server.json      # Server configuration file
│   ├── Exceptions/          # Custom exception classes (to be extended)
│   ├── Interface/           # Service interface definitions
│   │   ├── IDesktopService.cs   # Desktop service interface
│   │   ├── IFileSystemService.cs # File system service interface
│   │   └── IOcrService.cs       # OCR service interface
│   ├── Models/              # Data models (to be extended)
│   ├── Prompts/             # Prompt templates (to be extended)
│   ├── Services/            # Core service implementations
│   │   ├── DesktopService.cs    # Desktop operation service
│   │   ├── FileSystemService.cs # File system service
│   │   └── OcrService.cs        # OCR service
│   ├── Tools/               # MCP tool implementations
│   │   ├── Desktop/             # Desktop operation tools
│   │   │   ├── ClickTool.cs         # Click tool
│   │   │   ├── ClipboardTool.cs     # Clipboard tool
│   │   │   ├── DragTool.cs          # Drag tool
│   │   │   ├── GetWindowInfoTool.cs # Window info tool
│   │   │   ├── KeyTool.cs           # Key press tool
│   │   │   ├── LaunchTool.cs        # Application launch tool
│   │   │   ├── MoveTool.cs          # Mouse move tool
│   │   │   ├── OpenBrowserTool.cs   # Browser open tool
│   │   │   ├── PowershellTool.cs    # PowerShell execution tool
│   │   │   ├── ResizeTool.cs        # Window resize tool
│   │   │   ├── ScrapeTool.cs        # Web scraping tool
│   │   │   ├── ScreenshotTool.cs    # Screenshot tool
│   │   │   ├── ScrollTool.cs        # Scroll tool
│   │   │   ├── ShortcutTool.cs      # Keyboard shortcut tool
│   │   │   ├── StateTool.cs         # Desktop state tool
│   │   │   ├── SwitchTool.cs        # Application switch tool
│   │   │   ├── TypeTool.cs          # Text input tool
│   │   │   ├── UIElementTool.cs     # UI element operation tool
│   │   │   └── WaitTool.cs          # Wait tool
│   │   ├── FileSystem/          # File system tools
│   │   │   ├── CopyFileTool.cs      # File copy tool
│   │   │   ├── CreateDirectoryTool.cs # Directory creation tool
│   │   │   ├── CreateFileTool.cs    # File creation tool
│   │   │   ├── DeleteDirectoryTool.cs # Directory deletion tool
│   │   │   ├── DeleteFileTool.cs    # File deletion tool
│   │   │   ├── GetFileInfoTool.cs   # File info tool
│   │   │   ├── ListDirectoryTool.cs # Directory listing tool
│   │   │   ├── MoveFileTool.cs      # File move tool
│   │   │   ├── ReadFileTool.cs      # File read tool
│   │   │   ├── SearchFilesTool.cs   # File search tool
│   │   │   └── WriteFileTool.cs     # File write tool
│   │   └── OCR/                 # OCR recognition tools
│   │       ├── ExtractTextFromRegionTool.cs # Region text extraction tool
│   │       ├── ExtractTextFromScreenTool.cs # Screen text extraction tool
│   │       ├── FindTextOnScreenTool.cs      # Screen text finding tool
│   │       └── GetTextCoordinatesTool.cs    # Text coordinates tool
│   ├── Program.cs           # Program entry point
│   └── Windows-MCP.Net.csproj   # Project file
└── Windows-MCP.Net.Test/    # Test project
    ├── DesktopToolsExtendedTest.cs  # Desktop tools extended tests
    ├── FileSystemToolsExtendedTest.cs # File system tools extended tests
    ├── OCRToolsExtendedTest.cs      # OCR tools extended tests
    ├── ToolTest.cs                  # Basic tool tests
    ├── UIElementToolTest.cs         # UI element tool tests
    └── Windows-MCP.Net.Test.csproj  # Test project file
```

## 🔧 Configuration

### Logging Configuration

The project uses Serilog for logging, with log files saved in the `logs/` directory:

- Console output: Real-time log display
- File output: Daily rolling, retain 31 days
- Log level: Debug and above

### Environment Variables

| Variable | Description | Default |
|----------|-------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |

## 📝 License

This project is open source under the MIT License. See the [LICENSE](LICENSE) file for details.

## 🔗 Related Links

- [Model Context Protocol](https://modelcontextprotocol.io/)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Windows API Documentation](https://docs.microsoft.com/windows/win32/)

## 📞 Support

If you encounter issues or have suggestions, please:

1. Check [Issues](https://github.com/xuzeyu91/Windows-MCP.Net/issues)
2. Create a new Issue
3. Participate in discussions

---

**Note**: This tool requires appropriate Windows permissions to perform desktop automation operations. Please ensure use in a trusted environment.