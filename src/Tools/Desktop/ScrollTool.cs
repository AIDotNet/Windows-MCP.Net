using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;

namespace Tools.Desktop;

/// <summary>
/// MCP tool for scrolling operations.
/// </summary>
[McpServerToolType]
public class ScrollTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<ScrollTool> _logger;

    public ScrollTool(IDesktopService desktopService, ILogger<ScrollTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Scroll at specific coordinates or current mouse position.
    /// </summary>
    /// <param name="x">X coordinate (optional, uses current mouse position if not provided)</param>
    /// <param name="y">Y coordinate (optional, uses current mouse position if not provided)</param>
    /// <param name="type">Scroll type: "horizontal" or "vertical"</param>
    /// <param name="direction">Scroll direction: "up", "down", "left", or "right"</param>
    /// <param name="wheelTimes">Number of wheel scrolls (1 wheel = ~3-5 lines)</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Scroll at specific coordinates or current mouse position")]
    public async Task<string> ScrollAsync(
        [Description("X coordinate (optional, uses current mouse position if not provided)")] int? x = null,
        [Description("Y coordinate (optional, uses current mouse position if not provided)")] int? y = null,
        [Description("Scroll type: \"horizontal\" or \"vertical\"")] string type = "vertical",
        [Description("Scroll direction: \"up\", \"down\", \"left\", or \"right\"")] string direction = "down",
        [Description("Number of wheel scrolls (1 wheel = ~3-5 lines)")] int wheelTimes = 1)
    {
        _logger.LogInformation("Scrolling {Type} {Direction} at ({X},{Y}) for {WheelTimes} times", type, direction, x, y, wheelTimes);
        
        return await _desktopService.ScrollAsync(x, y, type, direction, wheelTimes);
    }
}