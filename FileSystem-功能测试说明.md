# Windows MCP.Net 文件系统功能测试说明

## 概述

本文档描述了新添加的文件系统功能及其测试方法。文件系统服务作为独立的服务实现，提供了完整的文件和目录操作功能。

## 新增的服务和工具

### 服务

- **IFileSystemService**: 文件系统服务接口
- **FileSystemService**: 文件系统服务实现类

### 工具类

1. **CreateFileTool**: 创建文件工具
2. **ReadFileTool**: 读取文件工具
3. **WriteFileTool**: 写入文件工具
4. **DeleteFileTool**: 删除文件工具
5. **CopyFileTool**: 复制文件工具
6. **MoveFileTool**: 移动文件工具
7. **ListDirectoryTool**: 列出目录内容工具
8. **CreateDirectoryTool**: 创建目录工具
9. **DeleteDirectoryTool**: 删除目录工具
10. **GetFileInfoTool**: 获取文件信息工具
11. **SearchFilesTool**: 搜索文件工具

## 功能特性

### 文件操作

- ✅ 创建文件（支持自动创建父目录）
- ✅ 读取文件内容（UTF-8编码）
- ✅ 写入文件内容（支持覆盖和追加模式）
- ✅ 删除文件
- ✅ 复制文件（支持覆盖选项）
- ✅ 移动文件（支持覆盖选项）
- ✅ 获取文件详细信息（大小、创建时间、修改时间等）
- ✅ 检查文件是否存在
- ✅ 获取文件大小

### 目录操作

- ✅ 创建目录（支持递归创建父目录）
- ✅ 删除目录（支持递归删除）
- ✅ 列出目录内容（支持递归列出、过滤文件/目录）
- ✅ 获取目录详细信息
- ✅ 检查目录是否存在

### 搜索功能

- ✅ 按文件名模式搜索（支持通配符 * 和 ?）
- ✅ 按文件扩展名搜索
- ✅ 支持递归搜索
- ✅ 搜索结果包含文件大小和修改时间

## 安全特性

- ✅ 路径验证和规范化
- ✅ 异常处理和错误日志记录
- ✅ 操作权限检查
- ✅ 防止意外覆盖（可选覆盖参数）

## 测试建议

### 基本文件操作测试

```json
{
  "tool": "create_file",
  "params": {
    "path": "C:\\temp\\test.txt",
    "content": "Hello, World!"
  }
}
```

```json
{
  "tool": "read_file",
  "params": {
    "path": "C:\\temp\\test.txt"
  }
}
```

```json
{
  "tool": "write_file",
  "params": {
    "path": "C:\\temp\\test.txt",
    "content": "\nAppended content",
    "append": true
  }
}
```

### 目录操作测试

```json
{
  "tool": "create_directory",
  "params": {
    "path": "C:\\temp\\testdir",
    "createParents": true
  }
}
```

```json
{
  "tool": "list_directory",
  "params": {
    "path": "C:\\temp",
    "includeFiles": true,
    "includeDirectories": true,
    "recursive": false
  }
}
```

### 搜索功能测试

```json
{
  "tool": "search_files_by_name",
  "params": {
    "directory": "C:\\temp",
    "pattern": "*.txt",
    "recursive": true
  }
}
```

```json
{
  "tool": "search_files_by_extension",
  "params": {
    "directory": "C:\\temp",
    "extension": ".txt",
    "recursive": true
  }
}
```

### 文件信息测试

```json
{
  "tool": "get_file_info",
  "params": {
    "path": "C:\\temp\\test.txt"
  }
}
```

## 日志记录

所有文件系统操作都会记录详细的日志信息，包括：

- 操作类型和参数
- 操作结果（成功/失败）
- 错误信息（如果有）
- 执行时间戳

## 错误处理

文件系统服务提供了完善的错误处理机制：

- 路径验证（空路径、无效路径）
- 权限检查（读写权限）
- 文件/目录存在性检查
- 异常捕获和友好错误消息
- 状态码返回（0表示成功，1表示失败）

## 性能优化

- 使用异步操作避免阻塞
- 文件大小格式化显示（B, KB, MB, GB, TB）
- 相对路径显示优化
- 批量操作支持

## 兼容性

- 支持 Windows 路径格式
- UTF-8 编码支持
- .NET 10.0 兼容
- 与现有 MCP 协议完全兼容

## 部署说明

1. 文件系统服务已在 `Program.cs` 中注册为单例服务
2. 所有工具类会自动通过反射注册到 MCP 服务器
3. 无需额外配置即可使用

## 后续扩展建议

- 添加文件监控功能（FileSystemWatcher）
- 支持文件压缩和解压缩
- 添加文件内容搜索功能
- 支持文件权限管理
- 添加文件同步功能
- 支持网络路径操作

---

**注意**: 在生产环境中使用时，请确保有适当的权限控制和安全措施。