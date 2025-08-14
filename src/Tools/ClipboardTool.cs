using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;

namespace WindowsMCP.Net.Tools;

/// <summary>
/// MCP tool for clipboard operations.
/// </summary>
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
    /// <param name="text">The text to copy (required for copy mode)</param>
    /// <returns>Result message</returns>

    public async Task<string> ClipboardOperationAsync(
        [Description("The mode: \"copy\" to copy text, \"paste\" to retrieve clipboard content")] string mode,
        [Description("The text to copy (required for copy mode)")] string? text = null)
    {
        _logger.LogInformation("Clipboard operation: {Mode}", mode);
        
        return await _desktopService.ClipboardOperationAsync(mode, text);
    }
}