using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interface;
using Microsoft.Extensions.Logging;

namespace WindowsMCP.Net.Services;

/// <summary>
/// Implementation of file system operations.
/// Provides methods for file and directory management.
/// </summary>
public class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;

    /// <summary>
    /// 初始化文件系统服务实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public FileSystemService(ILogger<FileSystemService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 创建新文件并写入指定内容
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="content">文件内容</param>
    /// <returns>包含响应消息和状态码的元组</returns>
    public async Task<(string Response, int Status)> CreateFileAsync(string path, string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ("File path cannot be empty", 1);
            }

            // 确保目录存在
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(path))
            {
                return ($"File already exists: {path}", 1);
            }

            await File.WriteAllTextAsync(path, content ?? string.Empty, Encoding.UTF8);
            _logger.LogInformation("Created file: {Path}", path);
            return ($"Successfully created file: {path}", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating file {Path}", path);
            return ($"Error creating file: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>包含文件内容和状态码的元组</returns>
    public async Task<(string Content, int Status)> ReadFileAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ("File path cannot be empty", 1);
            }

            if (!File.Exists(path))
            {
                return ($"File not found: {path}", 1);
            }

            var content = await File.ReadAllTextAsync(path, Encoding.UTF8);
            _logger.LogInformation("Read file: {Path}", path);
            return (content, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file {Path}", path);
            return ($"Error reading file: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 写入内容到文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="content">要写入的内容</param>
    /// <param name="append">是否追加到现有内容</param>
    /// <returns>包含响应消息和状态码的元组</returns>
    public async Task<(string Response, int Status)> WriteFileAsync(string path, string content, bool append = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ("File path cannot be empty", 1);
            }

            // 确保目录存在
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (append)
            {
                await File.AppendAllTextAsync(path, content ?? string.Empty, Encoding.UTF8);
                _logger.LogInformation("Appended to file: {Path}", path);
                return ($"Successfully appended to file: {path}", 0);
            }
            else
            {
                await File.WriteAllTextAsync(path, content ?? string.Empty, Encoding.UTF8);
                _logger.LogInformation("Wrote to file: {Path}", path);
                return ($"Successfully wrote to file: {path}", 0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to file {Path}", path);
            return ($"Error writing to file: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>包含响应消息和状态码的元组</returns>
    public async Task<(string Response, int Status)> DeleteFileAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ("File path cannot be empty", 1);
            }

            if (!File.Exists(path))
            {
                return ($"File not found: {path}", 1);
            }

            File.Delete(path);
            _logger.LogInformation("Deleted file: {Path}", path);
            return ($"Successfully deleted file: {path}", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {Path}", path);
            return ($"Error deleting file: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="source">源文件路径</param>
    /// <param name="destination">目标文件路径</param>
    /// <param name="overwrite">是否覆盖已存在的文件</param>
    /// <returns>包含响应消息和状态码的元组</returns>
    public async Task<(string Response, int Status)> CopyFileAsync(string source, string destination, bool overwrite = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
            {
                return ("Source and destination paths cannot be empty", 1);
            }

            if (!File.Exists(source))
            {
                return ($"Source file not found: {source}", 1);
            }

            if (File.Exists(destination) && !overwrite)
            {
                return ($"Destination file already exists: {destination}. Use overwrite=true to replace it.", 1);
            }

            // 确保目标目录存在
            var directory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(source, destination, overwrite);
            _logger.LogInformation("Copied file from {Source} to {Destination}", source, destination);
            return ($"Successfully copied file from {source} to {destination}", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying file from {Source} to {Destination}", source, destination);
            return ($"Error copying file: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 移动文件
    /// </summary>
    /// <param name="source">源文件路径</param>
    /// <param name="destination">目标文件路径</param>
    /// <param name="overwrite">是否覆盖已存在的文件</param>
    /// <returns>包含响应消息和状态码的元组</returns>
    public async Task<(string Response, int Status)> MoveFileAsync(string source, string destination, bool overwrite = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
            {
                return ("Source and destination paths cannot be empty", 1);
            }

            if (!File.Exists(source))
            {
                return ($"Source file not found: {source}", 1);
            }

            if (File.Exists(destination) && !overwrite)
            {
                return ($"Destination file already exists: {destination}. Use overwrite=true to replace it.", 1);
            }

            // 确保目标目录存在
            var directory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (overwrite && File.Exists(destination))
            {
                File.Delete(destination);
            }

            File.Move(source, destination);
            _logger.LogInformation("Moved file from {Source} to {Destination}", source, destination);
            return ($"Successfully moved file from {source} to {destination}", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving file from {Source} to {Destination}", source, destination);
            return ($"Error moving file: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 列出目录内容
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <param name="includeFiles">是否包含文件</param>
    /// <param name="includeDirectories">是否包含目录</param>
    /// <param name="recursive">是否递归列出</param>
    /// <returns>包含目录列表和状态码的元组</returns>
    public async Task<(string Listing, int Status)> ListDirectoryAsync(string path, bool includeFiles = true, bool includeDirectories = true, bool recursive = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ("Directory path cannot be empty", 1);
            }

            if (!Directory.Exists(path))
            {
                return ($"Directory not found: {path}", 1);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Contents of directory: {path}");
            sb.AppendLine();

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            if (includeDirectories)
            {
                var directories = Directory.GetDirectories(path, "*", searchOption);
                if (directories.Length > 0)
                {
                    sb.AppendLine("Directories:");
                    foreach (var dir in directories.OrderBy(d => d))
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        var relativePath = Path.GetRelativePath(path, dir);
                        sb.AppendLine($"  📁 {relativePath} (Created: {dirInfo.CreationTime:yyyy-MM-dd HH:mm:ss})");
                    }
                    sb.AppendLine();
                }
            }

            if (includeFiles)
            {
                var files = Directory.GetFiles(path, "*", searchOption);
                if (files.Length > 0)
                {
                    sb.AppendLine("Files:");
                    foreach (var file in files.OrderBy(f => f))
                    {
                        var fileInfo = new FileInfo(file);
                        var relativePath = Path.GetRelativePath(path, file);
                        var sizeStr = FormatFileSize(fileInfo.Length);
                        sb.AppendLine($"  📄 {relativePath} ({sizeStr}, Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss})");
                    }
                }
            }

            _logger.LogInformation("Listed directory: {Path}", path);
            return (sb.ToString(), 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing directory {Path}", path);
            return ($"Error listing directory: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <param name="createParents">是否创建父目录</param>
    /// <returns>包含响应消息和状态码的元组</returns>
    public async Task<(string Response, int Status)> CreateDirectoryAsync(string path, bool createParents = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ("Directory path cannot be empty", 1);
            }

            if (Directory.Exists(path))
            {
                return ($"Directory already exists: {path}", 1);
            }

            if (createParents)
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                var parentDir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                {
                    return ($"Parent directory does not exist: {parentDir}", 1);
                }
                Directory.CreateDirectory(path);
            }

            _logger.LogInformation("Created directory: {Path}", path);
            return ($"Successfully created directory: {path}", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory {Path}", path);
            return ($"Error creating directory: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 删除目录
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <param name="recursive">是否递归删除</param>
    /// <returns>包含响应消息和状态码的元组</returns>
    public async Task<(string Response, int Status)> DeleteDirectoryAsync(string path, bool recursive = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ("Directory path cannot be empty", 1);
            }

            if (!Directory.Exists(path))
            {
                return ($"Directory not found: {path}", 1);
            }

            Directory.Delete(path, recursive);
            _logger.LogInformation("Deleted directory: {Path}", path);
            return ($"Successfully deleted directory: {path}", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting directory {Path}", path);
            return ($"Error deleting directory: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 获取文件或目录信息
    /// </summary>
    /// <param name="path">文件或目录路径</param>
    /// <returns>包含文件/目录信息和状态码的元组</returns>
    public async Task<(string Info, int Status)> GetFileInfoAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ("Path cannot be empty", 1);
            }

            var sb = new StringBuilder();

            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                sb.AppendLine($"File Information: {path}");
                sb.AppendLine($"Type: File");
                sb.AppendLine($"Size: {FormatFileSize(fileInfo.Length)} ({fileInfo.Length:N0} bytes)");
                sb.AppendLine($"Created: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Accessed: {fileInfo.LastAccessTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Attributes: {fileInfo.Attributes}");
                sb.AppendLine($"Extension: {fileInfo.Extension}");
                sb.AppendLine($"Directory: {fileInfo.DirectoryName}");
            }
            else if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);
                var fileCount = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
                var dirCount = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Length;
                
                sb.AppendLine($"Directory Information: {path}");
                sb.AppendLine($"Type: Directory");
                sb.AppendLine($"Created: {dirInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Modified: {dirInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Accessed: {dirInfo.LastAccessTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Attributes: {dirInfo.Attributes}");
                sb.AppendLine($"Files: {fileCount:N0}");
                sb.AppendLine($"Subdirectories: {dirCount:N0}");
                sb.AppendLine($"Parent: {dirInfo.Parent?.FullName ?? "(Root)"}");
            }
            else
            {
                return ($"Path not found: {path}", 1);
            }

            _logger.LogInformation("Got info for: {Path}", path);
            return (sb.ToString(), 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting info for {Path}", path);
            return ($"Error getting file info: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>文件是否存在</returns>
    public bool FileExists(string path)
    {
        return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
    }

    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns>目录是否存在</returns>
    public bool DirectoryExists(string path)
    {
        return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
    }

    /// <summary>
    /// 获取文件大小
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>包含文件大小和状态码的元组</returns>
    public async Task<(long Size, int Status)> GetFileSizeAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return (0, 1);
            }

            if (!File.Exists(path))
            {
                return (0, 1);
            }

            var fileInfo = new FileInfo(path);
            return (fileInfo.Length, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file size for {Path}", path);
            return (0, 1);
        }
    }

    /// <summary>
    /// 按名称模式搜索文件
    /// </summary>
    /// <param name="directory">搜索目录</param>
    /// <param name="pattern">搜索模式</param>
    /// <param name="recursive">是否递归搜索</param>
    /// <returns>包含搜索结果和状态码的元组</returns>
    public async Task<(string Results, int Status)> SearchFilesByNameAsync(string directory, string pattern, bool recursive = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(pattern))
            {
                return ("Directory and pattern cannot be empty", 1);
            }

            if (!Directory.Exists(directory))
            {
                return ($"Directory not found: {directory}", 1);
            }

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(directory, pattern, searchOption);

            var sb = new StringBuilder();
            sb.AppendLine($"Search Results for pattern '{pattern}' in '{directory}' (Recursive: {recursive}):");
            sb.AppendLine($"Found {files.Length} file(s):");
            sb.AppendLine();

            foreach (var file in files.OrderBy(f => f))
            {
                var fileInfo = new FileInfo(file);
                var relativePath = Path.GetRelativePath(directory, file);
                var sizeStr = FormatFileSize(fileInfo.Length);
                sb.AppendLine($"📄 {relativePath} ({sizeStr}, Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss})");
            }

            _logger.LogInformation("Searched files by name in {Directory} with pattern {Pattern}", directory, pattern);
            return (sb.ToString(), 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching files by name in {Directory} with pattern {Pattern}", directory, pattern);
            return ($"Error searching files: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 按扩展名搜索文件
    /// </summary>
    /// <param name="directory">搜索目录</param>
    /// <param name="extension">文件扩展名</param>
    /// <param name="recursive">是否递归搜索</param>
    /// <returns>包含搜索结果和状态码的元组</returns>
    public async Task<(string Results, int Status)> SearchFilesByExtensionAsync(string directory, string extension, bool recursive = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(extension))
            {
                return ("Directory and extension cannot be empty", 1);
            }

            if (!Directory.Exists(directory))
            {
                return ($"Directory not found: {directory}", 1);
            }

            // 确保扩展名以点开头
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            var pattern = "*" + extension;
            return await SearchFilesByNameAsync(directory, pattern, recursive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching files by extension in {Directory} with extension {Extension}", directory, extension);
            return ($"Error searching files: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 格式化文件大小显示
    /// </summary>
    /// <param name="bytes">字节数</param>
    /// <returns>格式化的文件大小字符串</returns>
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}