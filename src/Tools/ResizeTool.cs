using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;

namespace WindowsMCP.Net.Tools;

/// <summary>
/// MCP tool for resizing and moving application windows.
/// </summary>
public class ResizeTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<ResizeTool> _logger;

    public ResizeTool(IDesktopService desktopService, ILogger<ResizeTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Resize a specific application window to specific size or move to specific location.
    /// </summary>
    /// <param name="name">The name of the application window</param>
    /// <param name="width">New width (optional)</param>
    /// <param name="height">New height (optional)</param>
    /// <param name="x">New X position (optional)</param>
    /// <param name="y">New Y position (optional)</param>
    /// <returns>Result message</returns>

    public async Task<string> ResizeAppAsync(
        [Description("The name of the application window")] string name,
        [Description("New width (optional)")] int? width = null,
        [Description("New height (optional)")] int? height = null,
        [Description("New X position (optional)")] int? x = null,
        [Description("New Y position (optional)")] int? y = null)
    {
        _logger.LogInformation("Resizing window {Name} to size ({Width},{Height}) at position ({X},{Y})", name, width, height, x, y);
        
        var (response, status) = await _desktopService.ResizeAppAsync(name, width, height, x, y);
        
        if (status != 0)
        {
            var defaultLanguage = _desktopService.GetDefaultLanguage();
            return $"Failed to resize {name} window. Try to use the app name in the default language ({defaultLanguage}).";
        }
        
        return response;
    }
}