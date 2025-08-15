using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;

namespace WindowsMCP.Net.Tools.OCR;

/// <summary>
/// Tool for extracting text from a specific screen region using OCR.
/// </summary>
[McpServerToolType]
public class ExtractTextFromRegionTool
{
    private readonly IOcrService _ocrService;
    private readonly ILogger<ExtractTextFromRegionTool> _logger;

    /// <summary>
    /// 初始化区域文字识别工具
    /// </summary>
    /// <param name="ocrService">OCR服务</param>
    /// <param name="logger">日志记录器</param>
    public ExtractTextFromRegionTool(IOcrService ocrService, ILogger<ExtractTextFromRegionTool> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    /// <summary>
    /// 从屏幕指定区域提取文字
    /// </summary>
    /// <param name="x">区域左上角X坐标</param>
    /// <param name="y">区域左上角Y坐标</param>
    /// <param name="width">区域宽度</param>
    /// <param name="height">区域高度</param>
    /// <returns>包含识别结果的JSON字符串</returns>
    [McpServerTool, Description("Extract text from a specific region of the screen using OCR")]
    public async Task<string> ExtractTextFromRegionAsync(
        [Description("X coordinate of the region's top-left corner")] int x,
        [Description("Y coordinate of the region's top-left corner")] int y,
        [Description("Width of the region")] int width,
        [Description("Height of the region")] int height)
    {
        try
        {
            _logger.LogInformation("Extracting text from region: ({X}, {Y}, {Width}, {Height})", x, y, width, height);
            
            var (text, status) = await _ocrService.ExtractTextFromRegionAsync(x, y, width, height);
            
            var result = new
            {
                success = status == 0,
                text = status == 0 ? text : string.Empty,
                message = status == 0 ? "Text extracted successfully" : "Failed to extract text",
                region = new { x, y, width, height }
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExtractTextFromRegionAsync");
            var errorResult = new
            {
                success = false,
                text = string.Empty,
                message = $"Error extracting text: {ex.Message}",
                region = new { x, y, width, height }
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}