using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;

namespace Tools.FileSystem;

/// <summary>
/// Tool for reading file content.
/// </summary>
[McpServerToolType]
public class ReadFileTool
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<ReadFileTool> _logger;

    /// <summary>
    /// 初始化读取文件工具
    /// </summary>
    /// <param name="fileSystemService">文件系统服务</param>
    /// <param name="logger">日志记录器</param>
    public ReadFileTool(IFileSystemService fileSystemService, ILogger<ReadFileTool> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>包含文件内容的JSON字符串</returns>
    [McpServerTool, Description("Read content from a file")]
    public async Task<string> ReadFileAsync(
        [Description("The file path to read")] string path)
    {
        try
        {
            _logger.LogInformation("Reading file: {Path}", path);
            
            var (content, status) = await _fileSystemService.ReadFileAsync(path);
            
            var result = new
            {
                success = status == 0,
                content = status == 0 ? content : null,
                message = status == 0 ? "File read successfully" : content,
                path,
                contentLength = status == 0 ? content.Length : 0
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReadFileAsync");
            var errorResult = new
            {
                success = false,
                content = (string?)null,
                message = $"Error reading file: {ex.Message}",
                path
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}