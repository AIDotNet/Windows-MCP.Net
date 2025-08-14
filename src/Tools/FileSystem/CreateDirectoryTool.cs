using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WindowsMCP.Net.Services;

namespace Tools.FileSystem;

/// <summary>
/// Tool for creating directories.
/// </summary>
public class CreateDirectoryTool
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<CreateDirectoryTool> _logger;

    /// <summary>
    /// 初始化创建目录工具
    /// </summary>
    /// <param name="fileSystemService">文件系统服务</param>
    /// <param name="logger">日志记录器</param>
    public CreateDirectoryTool(IFileSystemService fileSystemService, ILogger<CreateDirectoryTool> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <param name="createParents">是否创建父目录，默认为true</param>
    /// <returns>包含操作结果的JSON字符串</returns>
    [Description("Create a new directory")]
    public async Task<string> CreateDirectoryAsync(
        [Description("The directory path to create")] string path,
        [Description("Whether to create parent directories if they don't exist")] bool createParents = true)
    {
        try
        {
            _logger.LogInformation("Creating directory: {Path}, CreateParents: {CreateParents}", path, createParents);
            
            var (response, status) = await _fileSystemService.CreateDirectoryAsync(path, createParents);
            
            var result = new
            {
                success = status == 0,
                message = response,
                path,
                createParents
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateDirectoryAsync");
            var errorResult = new
            {
                success = false,
                message = $"Error creating directory: {ex.Message}",
                path,
                createParents
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}