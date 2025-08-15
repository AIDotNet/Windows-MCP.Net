using System.ComponentModel;
using System.Text.Json;
using Interface;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Tools.FileSystem;

/// <summary>
/// Tool for listing directory contents.
/// </summary>
[McpServerToolType]
public class ListDirectoryTool
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<ListDirectoryTool> _logger;

    /// <summary>
    /// 初始化列出目录工具
    /// </summary>
    /// <param name="fileSystemService">文件系统服务</param>
    /// <param name="logger">日志记录器</param>
    public ListDirectoryTool(IFileSystemService fileSystemService, ILogger<ListDirectoryTool> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    /// <summary>
    /// 列出目录内容
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <param name="includeFiles">是否包含文件，默认为true</param>
    /// <param name="includeDirectories">是否包含目录，默认为true</param>
    /// <param name="recursive">是否递归列出，默认为false</param>
    /// <returns>包含目录列表的JSON字符串</returns>
    [McpServerTool, Description("List contents of a directory")]
    public async Task<string> ListDirectoryAsync(
        [Description("The directory path to list")] string path,
        [Description("Whether to include files in the listing")] bool includeFiles = true,
        [Description("Whether to include directories in the listing")] bool includeDirectories = true,
        [Description("Whether to list recursively")] bool recursive = false)
    {
        try
        {
            _logger.LogInformation("Listing directory: {Path}, Files: {IncludeFiles}, Directories: {IncludeDirectories}, Recursive: {Recursive}", 
                path, includeFiles, includeDirectories, recursive);
            
            var (listing, status) = await _fileSystemService.ListDirectoryAsync(path, includeFiles, includeDirectories, recursive);
            
            var result = new
            {
                success = status == 0,
                listing = status == 0 ? listing : null,
                message = status == 0 ? "Directory listed successfully" : listing,
                path,
                includeFiles,
                includeDirectories,
                recursive
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ListDirectoryAsync");
            var errorResult = new
            {
                success = false,
                listing = (string?)null,
                message = $"Error listing directory: {ex.Message}",
                path,
                includeFiles,
                includeDirectories,
                recursive
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}