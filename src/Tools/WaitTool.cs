using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;

namespace WindowsMCP.Net.Tools;

/// <summary>
/// MCP tool for pausing execution.
/// </summary>
[McpServerToolType]
public class WaitTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<WaitTool> _logger;

    public WaitTool(IDesktopService desktopService, ILogger<WaitTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Pause execution for specified duration in seconds.
    /// </summary>
    /// <param name="duration">Duration in seconds to wait</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Pause execution for specified duration in seconds")]
    public async Task<string> WaitAsync(
        [Description("Duration in seconds to wait")] int duration)
    {
        _logger.LogInformation("Waiting for {Duration} seconds", duration);
        
        return await _desktopService.WaitAsync(duration);
    }
}