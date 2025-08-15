using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Interface;

namespace Tools.Desktop;

/// <summary>
/// MCP tool for opening URLs in the default browser.
/// </summary>
[McpServerToolType]
public class OpenBrowserTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<OpenBrowserTool> _logger;

    public OpenBrowserTool(IDesktopService desktopService, ILogger<OpenBrowserTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Open a URL in the default browser.
    /// </summary>
    /// <param name="url">The URL to open. If not provided or invalid, opens Baidu</param>
    /// <param name="searchQuery">Optional search query to append to Baidu URL</param>
    /// <returns>Result message indicating success or failure</returns>
    [McpServerTool, Description("Open a URL in the default browser")]
    public async Task<string> OpenBrowserAsync(
        [Description("The URL to open (optional, defaults to Baidu if not provided)")] string? url = null,
        [Description("Optional search query to append to Baidu URL")] string? searchQuery = null)
    {
        _logger.LogInformation("Opening browser with URL: {Url}, SearchQuery: {SearchQuery}", url ?? "default", searchQuery ?? "none");
        
        return await _desktopService.OpenBrowserAsync(url, searchQuery);
    }
}