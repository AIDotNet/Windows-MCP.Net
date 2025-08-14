using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;

namespace WindowsMCP.Net.Tools;

/// <summary>
/// MCP tool for switching to application windows.
/// </summary>
[McpServerToolType]
public class SwitchTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<SwitchTool> _logger;

    public SwitchTool(IDesktopService desktopService, ILogger<SwitchTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Switch to a specific application window and bring it to foreground.
    /// </summary>
    /// <param name="name">The name of the application window</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Switch to a specific application window and bring it to foreground")]
    public async Task<string> SwitchAppAsync(
        [Description("The name of the application window")] string name)
    {
        _logger.LogInformation("Switching to window: {Name}", name);
        
        var (response, status) = await _desktopService.SwitchAppAsync(name);
        
        if (status != 0)
        {
            var defaultLanguage = _desktopService.GetDefaultLanguage();
            return $"Failed to switch to {name} window. Try to use the app name in the default language ({defaultLanguage}).";
        }
        
        return response;
    }
}