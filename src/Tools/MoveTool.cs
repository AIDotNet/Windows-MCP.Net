using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;

namespace WindowsMCP.Net.Tools;

/// <summary>
/// MCP tool for moving mouse cursor.
/// </summary>
[McpServerToolType]
public class MoveTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<MoveTool> _logger;

    public MoveTool(IDesktopService desktopService, ILogger<MoveTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Move mouse cursor to specific coordinates without clicking.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Move mouse cursor to specific coordinates without clicking")]
    public async Task<string> MoveAsync(
        [Description("X coordinate")] int x,
        [Description("Y coordinate")] int y)
    {
        _logger.LogInformation("Moving mouse to ({X},{Y})", x, y);
        
        return await _desktopService.MoveAsync(x, y);
    }
}