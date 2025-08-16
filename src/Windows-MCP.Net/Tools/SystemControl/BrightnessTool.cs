using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Interface;

namespace Tools.SystemControl;

/// <summary>
/// MCP tool for screen brightness control operations.
/// </summary>
[McpServerToolType]
public class BrightnessTool
{
    private readonly ISystemControlService _systemControlService;
    private readonly ILogger<BrightnessTool> _logger;

    public BrightnessTool(ISystemControlService systemControlService, ILogger<BrightnessTool> logger)
    {
        _systemControlService = systemControlService;
        _logger = logger;
    }

    /// <summary>
    /// Adjust screen brightness up or down.
    /// </summary>
    /// <param name="inc">true to increase brightness, false to decrease</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Adjust screen brightness up or down")]
    public async Task<string> SetBrightnessAsync(
        [Description("true to increase brightness, false to decrease")] bool inc)
    {
        _logger.LogInformation("Adjusting brightness: {Direction}", inc ? "increase" : "decrease");
        
        return await _systemControlService.SetBrightnessAsync(inc);
    }

    /// <summary>
    /// Set screen brightness to a specific percentage.
    /// </summary>
    /// <param name="percent">Brightness percentage (0-100)</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Set screen brightness to a specific percentage")]
    public async Task<string> SetBrightnessPercentAsync(
        [Description("Brightness percentage (0-100)")] int percent)
    {
        _logger.LogInformation("Setting brightness to {Percent}%", percent);
        
        return await _systemControlService.SetBrightnessPercentAsync(percent);
    }

    /// <summary>
    /// Get current screen brightness level.
    /// </summary>
    /// <returns>Current brightness as a percentage string</returns>
    [McpServerTool, Description("Get current screen brightness level")]
    public async Task<string> GetCurrentBrightnessAsync()
    {
        _logger.LogInformation("Getting current brightness level");
        
        byte brightness = await _systemControlService.GetCurrentBrightnessAsync();
        return $"Current brightness: {brightness}%";
    }
}