using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;

namespace Tools.Desktop;

/// <summary>
/// MCP tool for getting window position and size information.
/// </summary>
[McpServerToolType]
public class GetWindowInfoTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<GetWindowInfoTool> _logger;

    public GetWindowInfoTool(IDesktopService desktopService, ILogger<GetWindowInfoTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Get window position and size information for a specific application window.
    /// </summary>
    /// <param name="name">The name of the application window</param>
    /// <returns>Window information including position, size, and center coordinates</returns>
    [McpServerTool, Description("Get window position and size information for a specific application window")]
    public async Task<string> GetWindowInfoAsync(
        [Description("The name of the application window")] string name)
    {
        _logger.LogInformation("Getting window info for: {Name}", name);
        
        var (response, status) = await _desktopService.GetWindowInfoAsync(name);
        
        if (status != 0)
        {
            var defaultLanguage = _desktopService.GetDefaultLanguage();
            return $"Failed to get window info for {name}. Try to use the app name in the default language ({defaultLanguage}).";
        }
        
        return response;
    }
}