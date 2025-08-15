using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;
using HtmlAgilityPack;
using ReverseMarkdown;

namespace Tools.Desktop;

/// <summary>
/// MCP tool for web scraping.
/// </summary>
[McpServerToolType]
public class ScrapeTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<ScrapeTool> _logger;

    public ScrapeTool(IDesktopService desktopService, ILogger<ScrapeTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Fetch and convert webpage content to markdown format.
    /// </summary>
    /// <param name="url">The full URL including protocol (http/https) to scrape</param>
    /// <returns>Structured text content in markdown format</returns>
    [McpServerTool, Description("Fetch and convert webpage content to markdown format")]
    public async Task<string> ScrapeAsync(
        [Description("The full URL including protocol (http/https) to scrape")] string url)
    {
        _logger.LogInformation("Scraping URL: {Url}", url);
        
        return await _desktopService.ScrapeAsync(url);
    }
}