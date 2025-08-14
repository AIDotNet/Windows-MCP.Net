using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;

namespace Tools.FileSystem;

/// <summary>
/// Tool for getting file or directory information.
/// </summary>
[McpServerToolType]
public class GetFileInfoTool
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<GetFileInfoTool> _logger;

    /// <summary>
    /// 初始化获取文件信息工具
    /// </summary>
    /// <param name="fileSystemService">文件系统服务</param>
    /// <param name="logger">日志记录器</param>
    public GetFileInfoTool(IFileSystemService fileSystemService, ILogger<GetFileInfoTool> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    /// <summary>
    /// 获取文件或目录信息
    /// </summary>
    /// <param name="path">文件或目录路径</param>
    /// <returns>包含文件/目录信息的JSON字符串</returns>
    [McpServerTool, Description("Get information about a file or directory")]
    public async Task<string> GetFileInfoAsync(
        [Description("The file or directory path to get information about")] string path)
    {
        try
        {
            _logger.LogInformation("Getting info for: {Path}", path);
            
            var (info, status) = await _fileSystemService.GetFileInfoAsync(path);
            
            var result = new
            {
                success = status == 0,
                info = status == 0 ? info : null,
                message = status == 0 ? "Information retrieved successfully" : info,
                path,
                fileExists = _fileSystemService.FileExists(path),
                directoryExists = _fileSystemService.DirectoryExists(path)
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetFileInfoAsync");
            var errorResult = new
            {
                success = false,
                info = (string?)null,
                message = $"Error getting file info: {ex.Message}",
                path,
                fileExists = false,
                directoryExists = false
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}