using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;

namespace WindowsMCP.Net.Tools;

/// <summary>
/// MCP tool for mouse click operations.
/// </summary>
public class ClickTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<ClickTool> _logger;

    public ClickTool(IDesktopService desktopService, ILogger<ClickTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Click on UI elements at specific coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="button">Mouse button: "left", "right", or "middle"</param>
    /// <param name="clicks">Number of clicks (1 for single, 2 for double, 3 for triple)</param>
    /// <returns>Result message</returns>

    public async Task<string> ClickAsync(
        [Description("X coordinate")] int x,
        [Description("Y coordinate")] int y,
        [Description("Mouse button: \"left\", \"right\", or \"middle\"")] string button = "left",
        [Description("Number of clicks (1 for single, 2 for double, 3 for triple)")] int clicks = 1)
    {
        _logger.LogInformation("Clicking at ({X},{Y}) with {Button} button, {Clicks} clicks", x, y, button, clicks);
        
        return await _desktopService.ClickAsync(x, y, button, clicks);
    }
}