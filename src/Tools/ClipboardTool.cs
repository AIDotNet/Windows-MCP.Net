using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;

namespace WindowsMCP.Net.Tools;

/// <summary>
/// MCP tool for clipboard operations.
/// </summary>
[McpServerToolType]
public class ClipboardTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<ClipboardTool> _logger;

    public ClipboardTool(IDesktopService desktopService, ILogger<ClipboardTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Copy text to clipboard or retrieve current clipboard content.
    /// </summary>
    /// <param name="mode">The mode: "copy" to copy text, "paste" to retrieve clipboard content</param>
    /// <param name="text">Text to copy (only required when mode is "copy")</param>
    /// <returns>Result message or clipboard content</returns>
    [McpServerTool, Description("Copy text to clipboard or retrieve current clipboard content")]
    public async Task<string> ClipboardAsync(
        [Description("The mode: \"copy\" to copy text, \"paste\" to retrieve clipboard content")] string mode,
        [Description("The text to copy (required for copy mode)")] string? text = null)
    {
        _logger.LogInformation("Clipboard operation: {Mode}", mode);
        
        return await _desktopService.ClipboardOperationAsync(mode, text);
    }
}