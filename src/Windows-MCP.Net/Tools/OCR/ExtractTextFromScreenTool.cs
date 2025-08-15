using System.ComponentModel;
using System.Text.Json;
using Interface;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace WindowsMCP.Net.Tools.OCR;

/// <summary>
/// Tool for extracting text from the entire screen using OCR.
/// </summary>
[McpServerToolType]
public class ExtractTextFromScreenTool
{
    private readonly IOcrService _ocrService;
    private readonly ILogger<ExtractTextFromScreenTool> _logger;

    /// <summary>
    /// 初始化全屏文字识别工具
    /// </summary>
    /// <param name="ocrService">OCR服务</param>
    /// <param name="logger">日志记录器</param>
    public ExtractTextFromScreenTool(IOcrService ocrService, ILogger<ExtractTextFromScreenTool> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    /// <summary>
    /// 从整个屏幕提取文字
    /// </summary>
    /// <returns>包含识别结果的JSON字符串</returns>
    [McpServerTool, Description("Extract text from the entire screen using OCR")]
    public async Task<string> ExtractTextFromScreenAsync()
    {
        try
        {
            _logger.LogInformation("Extracting text from entire screen");
            
            var (text, status) = await _ocrService.ExtractTextFromScreenAsync();
            
            var result = new
            {
                success = status == 0,
                text = status == 0 ? text : string.Empty,
                message = status == 0 ? "Text extracted successfully from screen" : "Failed to extract text from screen"
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExtractTextFromScreenAsync");
            var errorResult = new
            {
                success = false,
                text = string.Empty,
                message = $"Error extracting text from screen: {ex.Message}"
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}