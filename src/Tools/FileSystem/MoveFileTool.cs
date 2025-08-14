using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WindowsMCP.Net.Services;

namespace Tools.FileSystem;

/// <summary>
/// Tool for moving files.
/// </summary>
public class MoveFileTool
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<MoveFileTool> _logger;

    /// <summary>
    /// 初始化移动文件工具
    /// </summary>
    /// <param name="fileSystemService">文件系统服务</param>
    /// <param name="logger">日志记录器</param>
    public MoveFileTool(IFileSystemService fileSystemService, ILogger<MoveFileTool> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    /// <summary>
    /// 移动文件
    /// </summary>
    /// <param name="source">源文件路径</param>
    /// <param name="destination">目标文件路径</param>
    /// <param name="overwrite">是否覆盖已存在的文件，默认为false</param>
    /// <returns>包含操作结果的JSON字符串</returns>
    [Description("Move a file from source to destination")]
    public async Task<string> MoveFileAsync(
        [Description("The source file path")] string source,
        [Description("The destination file path")] string destination,
        [Description("Whether to overwrite existing file")] bool overwrite = false)
    {
        try
        {
            _logger.LogInformation("Moving file from {Source} to {Destination}, Overwrite: {Overwrite}", source, destination, overwrite);
            
            var (response, status) = await _fileSystemService.MoveFileAsync(source, destination, overwrite);
            
            var result = new
            {
                success = status == 0,
                message = response,
                source,
                destination,
                overwrite
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MoveFileAsync");
            var errorResult = new
            {
                success = false,
                message = $"Error moving file: {ex.Message}",
                source,
                destination,
                overwrite
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}