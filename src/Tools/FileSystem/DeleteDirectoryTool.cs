using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WindowsMCP.Net.Services;

namespace Tools.FileSystem;

/// <summary>
/// Tool for deleting directories.
/// </summary>
public class DeleteDirectoryTool
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<DeleteDirectoryTool> _logger;

    /// <summary>
    /// 初始化删除目录工具
    /// </summary>
    /// <param name="fileSystemService">文件系统服务</param>
    /// <param name="logger">日志记录器</param>
    public DeleteDirectoryTool(IFileSystemService fileSystemService, ILogger<DeleteDirectoryTool> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    /// <summary>
    /// 删除目录
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <param name="recursive">是否递归删除，默认为false</param>
    /// <returns>包含操作结果的JSON字符串</returns>
    [Description("Delete a directory")]
    public async Task<string> DeleteDirectoryAsync(
        [Description("The directory path to delete")] string path,
        [Description("Whether to delete recursively (including all contents)")] bool recursive = false)
    {
        try
        {
            _logger.LogInformation("Deleting directory: {Path}, Recursive: {Recursive}", path, recursive);
            
            var (response, status) = await _fileSystemService.DeleteDirectoryAsync(path, recursive);
            
            var result = new
            {
                success = status == 0,
                message = response,
                path,
                recursive
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteDirectoryAsync");
            var errorResult = new
            {
                success = false,
                message = $"Error deleting directory: {ex.Message}",
                path,
                recursive
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}