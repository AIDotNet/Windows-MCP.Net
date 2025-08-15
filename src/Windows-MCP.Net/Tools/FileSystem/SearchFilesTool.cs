using System.ComponentModel;
using System.Text.Json;
using Interface;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Tools.FileSystem;

/// <summary>
/// Tool for searching files by name pattern or extension.
/// </summary>
[McpServerToolType]
public class SearchFilesTool
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<SearchFilesTool> _logger;

    /// <summary>
    /// 初始化搜索文件工具
    /// </summary>
    /// <param name="fileSystemService">文件系统服务</param>
    /// <param name="logger">日志记录器</param>
    public SearchFilesTool(IFileSystemService fileSystemService, ILogger<SearchFilesTool> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    /// <summary>
    /// 按名称模式搜索文件
    /// </summary>
    /// <param name="directory">搜索目录</param>
    /// <param name="pattern">搜索模式（支持通配符 * 和 ?）</param>
    /// <param name="recursive">是否递归搜索，默认为false</param>
    /// <returns>包含搜索结果的JSON字符串</returns>
    [McpServerTool, Description("Search for files by name pattern")]
    public async Task<string> SearchFilesByNameAsync(
        [Description("The directory to search in")] string directory,
        [Description("The search pattern (supports wildcards * and ?)")] string pattern,
        [Description("Whether to search recursively")] bool recursive = false)
    {
        try
        {
            _logger.LogInformation("Searching files by name in {Directory} with pattern {Pattern}, Recursive: {Recursive}", 
                directory, pattern, recursive);
            
            var (results, status) = await _fileSystemService.SearchFilesByNameAsync(directory, pattern, recursive);
            
            var result = new
            {
                success = status == 0,
                results = status == 0 ? results : null,
                message = status == 0 ? "Search completed successfully" : results,
                directory,
                pattern,
                recursive
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchFilesByNameAsync");
            var errorResult = new
            {
                success = false,
                results = (string?)null,
                message = $"Error searching files: {ex.Message}",
                directory,
                pattern,
                recursive
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// 按扩展名搜索文件
    /// </summary>
    /// <param name="directory">搜索目录</param>
    /// <param name="extension">文件扩展名（可以带或不带点）</param>
    /// <param name="recursive">是否递归搜索，默认为false</param>
    /// <returns>包含搜索结果的JSON字符串</returns>
    [McpServerTool, Description("Search for files by extension")]
    public async Task<string> SearchFilesByExtensionAsync(
        [Description("The directory to search in")] string directory,
        [Description("The file extension (with or without dot)")] string extension,
        [Description("Whether to search recursively")] bool recursive = false)
    {
        try
        {
            _logger.LogInformation("Searching files by extension in {Directory} with extension {Extension}, Recursive: {Recursive}", 
                directory, extension, recursive);
            
            var (results, status) = await _fileSystemService.SearchFilesByExtensionAsync(directory, extension, recursive);
            
            var result = new
            {
                success = status == 0,
                results = status == 0 ? results : null,
                message = status == 0 ? "Search completed successfully" : results,
                directory,
                extension,
                recursive
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchFilesByExtensionAsync");
            var errorResult = new
            {
                success = false,
                results = (string?)null,
                message = $"Error searching files: {ex.Message}",
                directory,
                extension,
                recursive
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}