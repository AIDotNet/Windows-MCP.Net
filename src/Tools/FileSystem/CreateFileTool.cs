using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WindowsMCP.Net.Services;

namespace Tools.FileSystem;

/// <summary>
/// Tool for creating new files with specified content.
/// </summary>
public class CreateFileTool
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<CreateFileTool> _logger;

    /// <summary>
    /// 初始化创建文件工具
    /// </summary>
    /// <param name="fileSystemService">文件系统服务</param>
    /// <param name="logger">日志记录器</param>
    public CreateFileTool(IFileSystemService fileSystemService, ILogger<CreateFileTool> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    /// <summary>
    /// 创建新文件并写入指定内容
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="content">文件内容，默认为空字符串</param>
    /// <returns>包含操作结果的JSON字符串</returns>
    [Description("Create a new file with specified content")]
    public async Task<string> CreateFileAsync(
        [Description("The file path to create")] string path,
        [Description("The content to write to the file")] string content = "")
    {
        try
        {
            _logger.LogInformation("Creating file: {Path}", path);
            
            var (response, status) = await _fileSystemService.CreateFileAsync(path, content);
            
            var result = new
            {
                success = status == 0,
                message = response,
                path,
                contentLength = content?.Length ?? 0
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateFileAsync");
            var errorResult = new
            {
                success = false,
                message = $"Error creating file: {ex.Message}",
                path
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}