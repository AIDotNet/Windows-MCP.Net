using System.ComponentModel;
using System.Text.Json;
using Interface;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace WindowsMCP.Net.Tools.OCR;

/// <summary>
/// Tool for getting coordinates of specific text on the screen using OCR.
/// </summary>
[McpServerToolType]
public class GetTextCoordinatesTool
{
    private readonly IOcrService _ocrService;
    private readonly ILogger<GetTextCoordinatesTool> _logger;

    /// <summary>
    /// 初始化文字坐标获取工具
    /// </summary>
    /// <param name="ocrService">OCR服务</param>
    /// <param name="logger">日志记录器</param>
    public GetTextCoordinatesTool(IOcrService ocrService, ILogger<GetTextCoordinatesTool> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    /// <summary>
    /// 获取指定文字在屏幕上的坐标
    /// </summary>
    /// <param name="text">要定位的文字</param>
    /// <returns>包含坐标信息的JSON字符串</returns>
    [McpServerTool, Description("Get the coordinates of specific text on the screen using OCR")]
    public async Task<string> GetTextCoordinatesAsync(
        [Description("The text to locate on the screen")] string text)
    {
        try
        {
            _logger.LogInformation("Getting coordinates for text: {Text}", text);
            
            var (coordinates, status) = await _ocrService.GetTextCoordinatesAsync(text);
            
            var result = new
            {
                success = status == 0,
                found = coordinates != null,
                searchText = text,
                coordinates = coordinates != null ? new { x = coordinates.Value.X, y = coordinates.Value.Y } : null,
                message = status == 0 
                    ? (coordinates != null 
                        ? $"Text '{text}' found at coordinates ({coordinates.Value.X}, {coordinates.Value.Y})"
                        : $"Text '{text}' not found on screen")
                    : "Failed to get text coordinates"
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetTextCoordinatesAsync");
            var errorResult = new
            {
                success = false,
                found = false,
                searchText = text,
                coordinates = (object?)null,
                message = $"Error getting text coordinates: {ex.Message}"
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}