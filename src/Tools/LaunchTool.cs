using Microsoft.Extensions.Logging;
using WindowsMCP.Net.Services;
using System.ComponentModel;
using ModelContextProtocol.Server;

namespace WindowsMCP.Net.Tools;

/// <summary>
/// MCP tool for launching applications from the Windows Start Menu.
/// </summary>
[McpServerToolType]
public class LaunchTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<LaunchTool> _logger;

    public LaunchTool(IDesktopService desktopService, ILogger<LaunchTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Launch an application from the Windows Start Menu by name.
    /// </summary>
    /// <param name="name">The name of the application to launch (e.g., "notepad", "calculator", "chrome")</param>
    /// <returns>Result message indicating success or failure</returns>
    [McpServerTool, Description("Launch an application from the Windows Start Menu by name")]
    public async Task<string> LaunchAppAsync(
        [Description("The name of the application to launch")] string name)
    {
        _logger.LogInformation("Launching application: {Name}", name);
        
        var (response, status) = await _desktopService.LaunchAppAsync(name);
        
        if (status != 0)
        {
            var defaultLanguage = _desktopService.GetDefaultLanguage();
            return $"Failed to launch {name}. Try to use the app name in the default language ({defaultLanguage}).";
        }
        
        return response;
    }
}