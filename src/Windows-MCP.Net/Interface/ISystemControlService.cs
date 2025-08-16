namespace Interface;

/// <summary>
/// Interface for Windows system control services.
/// Provides methods for controlling volume, screen brightness, and display resolution.
/// </summary>
public interface ISystemControlService
{
    /// <summary>
    /// Adjust system volume up or down.
    /// </summary>
    /// <param name="inc">true to increase volume, false to decrease</param>
    /// <returns>Result message</returns>
    Task<string> SetVolumeAsync(bool inc);

    /// <summary>
    /// Set system volume to a specific percentage.
    /// </summary>
    /// <param name="percent">Volume percentage (0-100)</param>
    /// <returns>Result message</returns>
    Task<string> SetVolumePercentAsync(int percent);

    /// <summary>
    /// Get current system volume level.
    /// </summary>
    /// <returns>Current volume as a float (0.0 to 1.0)</returns>
    Task<float> GetCurrentVolumeAsync();

    /// <summary>
    /// Set mute state for system audio.
    /// </summary>
    /// <param name="mute">true to mute, false to unmute</param>
    /// <returns>Result message</returns>
    Task<string> SetMuteStateAsync(bool mute);

    /// <summary>
    /// Adjust screen brightness up or down.
    /// </summary>
    /// <param name="inc">true to increase brightness, false to decrease</param>
    /// <returns>Result message</returns>
    Task<string> SetBrightnessAsync(bool inc);

    /// <summary>
    /// Set screen brightness to a specific percentage.
    /// </summary>
    /// <param name="percent">Brightness percentage (0-100)</param>
    /// <returns>Result message</returns>
    Task<string> SetBrightnessPercentAsync(int percent);

    /// <summary>
    /// Get current screen brightness level.
    /// </summary>
    /// <returns>Current brightness as a byte (0-100)</returns>
    Task<byte> GetCurrentBrightnessAsync();

    /// <summary>
    /// Set screen resolution.
    /// </summary>
    /// <param name="type">Resolution type: "high", "medium", or "low"</param>
    /// <returns>Result message</returns>
    Task<string> SetResolutionAsync(string type);
}