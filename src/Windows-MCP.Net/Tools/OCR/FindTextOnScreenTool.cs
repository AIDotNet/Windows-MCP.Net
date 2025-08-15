using System.ComponentModel;
using System.Text.Json;
using Interface;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace WindowsMCP.Net.Tools.OCR;

/// <summary>
/// Tool for finding specific text on the screen using OCR.
/// </summary>
[McpServerToolType]
public class FindTextOnScreenTool
{
    private readonly IOcrService _ocrService;
    private readonly ILogger<FindTextOnScreenTool> _logger;

    /// <summary>
    /// 初始化屏幕文字查找工具
    /// </summary>
    /// <param name="ocrService">OCR服务</param>
    /// <param name="logger">日志记录器</param>
    public FindTextOnScreenTool(IOcrService ocrService, ILogger<FindTextOnScreenTool> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    /// <summary>
    /// 在屏幕上查找指定文字
    /// </summary>
    /// <param name="text">要查找的文字</param>
    /// <returns>包含查找结果的JSON字符串</returns>
    [McpServerTool, Description("Find specific text on the screen using OCR")]
    public async Task<string> FindTextOnScreenAsync(
        [Description("The text to search for on the screen")] string text)
    {
        try
        {
            _logger.LogInformation("Searching for text on screen: {Text}", text);
            
            var (found, status) = await _ocrService.FindTextOnScreenAsync(text);
            
            var result = new
            {
                success = status == 0,
                found = status == 0 ? found : false,
                searchText = text,
                message = status == 0 
                    ? (found ? $"Text '{text}' found on screen" : $"Text '{text}' not found on screen")
                    : "Failed to search for text on screen"
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FindTextOnScreenAsync");
            var errorResult = new
            {
                success = false,
                found = false,
                searchText = text,
                message = $"Error searching for text: {ex.Message}"
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}