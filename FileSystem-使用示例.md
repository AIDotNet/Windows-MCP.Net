# Windows MCP.Net 文件系统功能使用示例

## 概述

本文档提供了使用新添加的文件系统功能的实际示例。所有示例都基于 MCP (Model Context Protocol) 协议格式。

## MCP 工具调用格式

所有文件系统工具都遵循标准的 MCP 工具调用格式：

```json
{
  "method": "tools/call",
  "params": {
    "name": "tool_name",
    "arguments": {
      "parameter1": "value1",
      "parameter2": "value2"
    }
  }
}
```

## 文件操作示例

### 1. 创建文件

```json
{
  "method": "tools/call",
  "params": {
    "name": "create_file",
    "arguments": {
      "path": "C:\\Users\\Public\\Documents\\test.txt",
      "content": "这是一个测试文件\n包含多行内容\n创建时间: 2024-01-01"
    }
  }
}
```

**预期响应:**
```json
{
  "success": true,
  "message": "Successfully created file: C:\\Users\\Public\\Documents\\test.txt",
  "path": "C:\\Users\\Public\\Documents\\test.txt",
  "contentLength": 45
}
```

### 2. 读取文件

```json
{
  "method": "tools/call",
  "params": {
    "name": "read_file",
    "arguments": {
      "path": "C:\\Users\\Public\\Documents\\test.txt"
    }
  }
}
```

**预期响应:**
```json
{
  "success": true,
  "content": "这是一个测试文件\n包含多行内容\n创建时间: 2024-01-01",
  "message": "File read successfully",
  "path": "C:\\Users\\Public\\Documents\\test.txt",
  "contentLength": 45
}
```

### 3. 追加内容到文件

```json
{
  "method": "tools/call",
  "params": {
    "name": "write_file",
    "arguments": {
      "path": "C:\\Users\\Public\\Documents\\test.txt",
      "content": "\n追加的新内容\n修改时间: 2024-01-02",
      "append": true
    }
  }
}
```

### 4. 复制文件

```json
{
  "method": "tools/call",
  "params": {
    "name": "copy_file",
    "arguments": {
      "source": "C:\\Users\\Public\\Documents\\test.txt",
      "destination": "C:\\Users\\Public\\Documents\\test_backup.txt",
      "overwrite": false
    }
  }
}
```

### 5. 移动文件

```json
{
  "method": "tools/call",
  "params": {
    "name": "move_file",
    "arguments": {
      "source": "C:\\Users\\Public\\Documents\\test_backup.txt",
      "destination": "C:\\Users\\Public\\Documents\\backup\\test_backup.txt",
      "overwrite": false
    }
  }
}
```

### 6. 获取文件信息

```json
{
  "method": "tools/call",
  "params": {
    "name": "get_file_info",
    "arguments": {
      "path": "C:\\Users\\Public\\Documents\\test.txt"
    }
  }
}
```

**预期响应:**
```json
{
  "success": true,
  "info": "File Information: C:\\Users\\Public\\Documents\\test.txt\nType: File\nSize: 1.23 KB (1,234 bytes)\nCreated: 2024-01-01 10:30:45\nModified: 2024-01-02 14:20:30\nAccessed: 2024-01-02 14:25:15\nAttributes: Archive\nExtension: .txt\nDirectory: C:\\Users\\Public\\Documents",
  "message": "Information retrieved successfully",
  "path": "C:\\Users\\Public\\Documents\\test.txt",
  "fileExists": true,
  "directoryExists": false
}
```

### 7. 删除文件

```json
{
  "method": "tools/call",
  "params": {
    "name": "delete_file",
    "arguments": {
      "path": "C:\\Users\\Public\\Documents\\test.txt"
    }
  }
}
```

## 目录操作示例

### 1. 创建目录

```json
{
  "method": "tools/call",
  "params": {
    "name": "create_directory",
    "arguments": {
      "path": "C:\\Users\\Public\\Documents\\MyProject\\Data\\Logs",
      "createParents": true
    }
  }
}
```

### 2. 列出目录内容

```json
{
  "method": "tools/call",
  "params": {
    "name": "list_directory",
    "arguments": {
      "path": "C:\\Users\\Public\\Documents",
      "includeFiles": true,
      "includeDirectories": true,
      "recursive": false
    }
  }
}
```

**预期响应:**
```json
{
  "success": true,
  "listing": "Contents of directory: C:\\Users\\Public\\Documents\n\nDirectories:\n  📁 MyProject (Created: 2024-01-01 10:30:45)\n  📁 backup (Created: 2024-01-01 11:15:20)\n\nFiles:\n  📄 document1.docx (2.5 MB, Modified: 2024-01-01 09:45:30)\n  📄 readme.txt (1.2 KB, Modified: 2024-01-01 10:00:15)",
  "message": "Directory listed successfully",
  "path": "C:\\Users\\Public\\Documents",
  "includeFiles": true,
  "includeDirectories": true,
  "recursive": false
}
```

### 3. 递归列出目录内容

```json
{
  "method": "tools/call",
  "params": {
    "name": "list_directory",
    "arguments": {
      "path": "C:\\Users\\Public\\Documents\\MyProject",
      "includeFiles": true,
      "includeDirectories": true,
      "recursive": true
    }
  }
}
```

### 4. 删除目录

```json
{
  "method": "tools/call",
  "params": {
    "name": "delete_directory",
    "arguments": {
      "path": "C:\\Users\\Public\\Documents\\MyProject\\Data\\Logs",
      "recursive": true
    }
  }
}
```

## 搜索功能示例

### 1. 按文件名模式搜索

```json
{
  "method": "tools/call",
  "params": {
    "name": "search_files_by_name",
    "arguments": {
      "directory": "C:\\Users\\Public\\Documents",
      "pattern": "*.txt",
      "recursive": true
    }
  }
}
```

**预期响应:**
```json
{
  "success": true,
  "results": "Search Results for pattern '*.txt' in 'C:\\Users\\Public\\Documents' (Recursive: true):\nFound 3 file(s):\n\n📄 readme.txt (1.2 KB, Modified: 2024-01-01 10:00:15)\n📄 backup\\test_backup.txt (1.5 KB, Modified: 2024-01-02 14:20:30)\n📄 MyProject\\notes.txt (856 B, Modified: 2024-01-01 16:45:22)",
  "message": "Search completed successfully",
  "directory": "C:\\Users\\Public\\Documents",
  "pattern": "*.txt",
  "recursive": true
}
```

### 2. 按扩展名搜索

```json
{
  "method": "tools/call",
  "params": {
    "name": "search_files_by_extension",
    "arguments": {
      "directory": "C:\\Users\\Public\\Documents",
      "extension": "docx",
      "recursive": true
    }
  }
}
```

### 3. 搜索特定文件名

```json
{
  "method": "tools/call",
  "params": {
    "name": "search_files_by_name",
    "arguments": {
      "directory": "C:\\Users\\Public\\Documents",
      "pattern": "*config*",
      "recursive": true
    }
  }
}
```

## 批量操作示例

### 创建项目结构

```json
[
  {
    "method": "tools/call",
    "params": {
      "name": "create_directory",
      "arguments": {
        "path": "C:\\Users\\Public\\Documents\\WebProject",
        "createParents": true
      }
    }
  },
  {
    "method": "tools/call",
    "params": {
      "name": "create_directory",
      "arguments": {
        "path": "C:\\Users\\Public\\Documents\\WebProject\\src",
        "createParents": true
      }
    }
  },
  {
    "method": "tools/call",
    "params": {
      "name": "create_directory",
      "arguments": {
        "path": "C:\\Users\\Public\\Documents\\WebProject\\assets",
        "createParents": true
      }
    }
  },
  {
    "method": "tools/call",
    "params": {
      "name": "create_file",
      "arguments": {
        "path": "C:\\Users\\Public\\Documents\\WebProject\\README.md",
        "content": "# Web Project\n\n这是一个示例Web项目。\n\n## 目录结构\n\n- src/: 源代码\n- assets/: 静态资源\n"
      }
    }
  },
  {
    "method": "tools/call",
    "params": {
      "name": "create_file",
      "arguments": {
        "path": "C:\\Users\\Public\\Documents\\WebProject\\src\\index.html",
        "content": "<!DOCTYPE html>\n<html>\n<head>\n    <title>示例页面</title>\n</head>\n<body>\n    <h1>Hello, World!</h1>\n</body>\n</html>"
      }
    }
  }
]
```

## 错误处理示例

### 尝试读取不存在的文件

```json
{
  "method": "tools/call",
  "params": {
    "name": "read_file",
    "arguments": {
      "path": "C:\\Users\\Public\\Documents\\nonexistent.txt"
    }
  }
}
```

**预期响应:**
```json
{
  "success": false,
  "content": null,
  "message": "File not found: C:\\Users\\Public\\Documents\\nonexistent.txt",
  "path": "C:\\Users\\Public\\Documents\\nonexistent.txt",
  "contentLength": 0
}
```

### 尝试在不存在的目录中创建文件（不自动创建父目录）

```json
{
  "method": "tools/call",
  "params": {
    "name": "create_directory",
    "arguments": {
      "path": "C:\\Users\\Public\\Documents\\NonExistent\\SubDir",
      "createParents": false
    }
  }
}
```

**预期响应:**
```json
{
  "success": false,
  "message": "Parent directory does not exist: C:\\Users\\Public\\Documents\\NonExistent",
  "path": "C:\\Users\\Public\\Documents\\NonExistent\\SubDir",
  "createParents": false
}
```

## 最佳实践

### 1. 路径处理
- 在 Windows 上使用双反斜杠 `\\` 或正斜杠 `/`
- 使用绝对路径以避免歧义
- 检查路径是否存在再进行操作

### 2. 错误处理
- 始终检查返回的 `success` 字段
- 在失败时查看 `message` 字段获取详细错误信息
- 对于文件操作，考虑权限问题

### 3. 性能优化
- 对于大量文件操作，考虑批量处理
- 使用递归搜索时注意目录深度
- 大文件操作时注意内存使用

### 4. 安全考虑
- 验证用户输入的路径
- 避免操作系统关键目录
- 在删除操作前进行确认

## 集成示例

### 与现有桌面自动化功能结合

```json
[
  {
    "method": "tools/call",
    "params": {
      "name": "create_file",
      "arguments": {
        "path": "C:\\Users\\Public\\Documents\\automation_log.txt",
        "content": "自动化任务开始\n时间: 2024-01-01 10:00:00\n"
      }
    }
  },
  {
    "method": "tools/call",
    "params": {
      "name": "launch_app",
      "arguments": {
        "name": "notepad"
      }
    }
  },
  {
    "method": "tools/call",
    "params": {
      "name": "write_file",
      "arguments": {
        "path": "C:\\Users\\Public\\Documents\\automation_log.txt",
        "content": "记事本已启动\n时间: 2024-01-01 10:00:05\n",
        "append": true
      }
    }
  }
]
```

这些示例展示了如何有效使用 Windows MCP.Net 的文件系统功能来执行各种文件和目录操作。