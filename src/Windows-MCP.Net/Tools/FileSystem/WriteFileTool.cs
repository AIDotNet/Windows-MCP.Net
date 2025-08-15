using System.ComponentModel;
using System.Text.Json;
using Interface;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Tools.FileSystem;

/// <summary>
/// Tool for writing content to files.
/// </summary>
[McpServerToolType]
public class WriteFileTool
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<WriteFileTool> _logger;

    /// <summary>
    /// 初始化写入文件工具
    /// </summary>
    /// <param name="fileSystemService">文件系统服务</param>
    /// <param name="logger">日志记录器</param>
    public WriteFileTool(IFileSystemService fileSystemService, ILogger<WriteFileTool> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    /// <summary>
    /// 写入内容到文件
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="content">要写入的内容</param>
    /// <param name="append">是否追加到现有内容，默认为false（覆盖）</param>
    /// <returns>包含操作结果的JSON字符串</returns>
    [McpServerTool, Description("Write content to a file")]
    public async Task<string> WriteFileAsync(
        [Description("The file path to write to")] string path,
        [Description("The content to write to the file")] string content,
        [Description("Whether to append to existing content (true) or overwrite (false)")] bool append = false)
    {
        try
        {
            _logger.LogInformation("Writing to file: {Path}, Append: {Append}", path, append);
            
            var (response, status) = await _fileSystemService.WriteFileAsync(path, content, append);
            
            var result = new
            {
                success = status == 0,
                message = response,
                path,
                contentLength = content?.Length ?? 0,
                append
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WriteFileAsync");
            var errorResult = new
            {
                success = false,
                message = $"Error writing to file: {ex.Message}",
                path,
                append
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}