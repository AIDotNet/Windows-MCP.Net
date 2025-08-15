using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;

namespace Tools.FileSystem;

/// <summary>
/// Tool for deleting files.
/// </summary>
[McpServerToolType]
public class DeleteFileTool
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<DeleteFileTool> _logger;

    /// <summary>
    /// 初始化删除文件工具
    /// </summary>
    /// <param name="fileSystemService">文件系统服务</param>
    /// <param name="logger">日志记录器</param>
    public DeleteFileTool(IFileSystemService fileSystemService, ILogger<DeleteFileTool> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="path">要删除的文件路径</param>
    /// <returns>包含操作结果的JSON字符串</returns>
    [McpServerTool, Description("Delete a file")]
    public async Task<string> DeleteFileAsync(
        [Description("The file path to delete")] string path)
    {
        try
        {
            _logger.LogInformation("Deleting file: {Path}", path);
            
            var (response, status) = await _fileSystemService.DeleteFileAsync(path);
            
            var result = new
            {
                success = status == 0,
                message = response,
                path
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteFileAsync");
            var errorResult = new
            {
                success = false,
                message = $"Error deleting file: {ex.Message}",
                path
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}