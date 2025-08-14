using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;

namespace WindowsMCP.Net.Tools;

/// <summary>
/// MCP tool for typing text into input fields.
/// </summary>
[McpServerToolType]
public class TypeTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<TypeTool> _logger;

    public TypeTool(IDesktopService desktopService, ILogger<TypeTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Type text into input fields, text areas, or focused elements.
    /// </summary>
    /// <param name="x">X coordinate of the target element</param>
    /// <param name="y">Y coordinate of the target element</param>
    /// <param name="text">Text to type</param>
    /// <param name="clear">Whether to clear existing text first</param>
    /// <param name="pressEnter">Whether to press Enter after typing</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Type text into input fields, text areas, or focused elements")]
    public async Task<string> TypeAsync(
        [Description("X coordinate of the target element")] int x,
        [Description("Y coordinate of the target element")] int y,
        [Description("Text to type")] string text,
        [Description("Whether to clear existing text first")] bool clear = false,
        [Description("Whether to press Enter after typing")] bool pressEnter = false)
    {
        _logger.LogInformation("Typing text at ({X},{Y}): {Text}", x, y, text);
        
        return await _desktopService.TypeAsync(x, y, text, clear, pressEnter);
    }
}