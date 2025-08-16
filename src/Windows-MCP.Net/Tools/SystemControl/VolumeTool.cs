using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Interface;

namespace Tools.SystemControl;

/// <summary>
/// MCP tool for system volume control operations.
/// </summary>
[McpServerToolType]
public class VolumeTool
{
    private readonly ISystemControlService _systemControlService;
    private readonly ILogger<VolumeTool> _logger;

    public VolumeTool(ISystemControlService systemControlService, ILogger<VolumeTool> logger)
    {
        _systemControlService = systemControlService;
        _logger = logger;
    }

    /// <summary>
    /// Adjust system volume up or down.
    /// </summary>
    /// <param name="inc">true to increase volume, false to decrease</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Adjust system volume up or down")]
    public async Task<string> SetVolumeAsync(
        [Description("true to increase volume, false to decrease")] bool inc)
    {
        _logger.LogInformation("Adjusting volume: {Direction}", inc ? "increase" : "decrease");
        
        return await _systemControlService.SetVolumeAsync(inc);
    }

    /// <summary>
    /// Set system volume to a specific percentage.
    /// </summary>
    /// <param name="percent">Volume percentage (0-100)</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Set system volume to a specific percentage")]
    public async Task<string> SetVolumePercentAsync(
        [Description("Volume percentage (0-100)")] int percent)
    {
        _logger.LogInformation("Setting volume to {Percent}%", percent);
        
        return await _systemControlService.SetVolumePercentAsync(percent);
    }

    /// <summary>
    /// Get current system volume level.
    /// </summary>
    /// <returns>Current volume as a percentage string</returns>
    [McpServerTool, Description("Get current system volume level")]
    public async Task<string> GetCurrentVolumeAsync()
    {
        _logger.LogInformation("Getting current volume level");
        
        float volume = await _systemControlService.GetCurrentVolumeAsync();
        int percentage = (int)(volume * 100);
        return $"Current volume: {percentage}%";
    }

    /// <summary>
    /// Set mute state for system audio.
    /// </summary>
    /// <param name="mute">true to mute, false to unmute</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Set mute state for system audio")]
    public async Task<string> SetMuteStateAsync(
        [Description("true to mute, false to unmute")] bool mute)
    {
        _logger.LogInformation("Setting mute state: {Mute}", mute);
        
        return await _systemControlService.SetMuteStateAsync(mute);
    }
}