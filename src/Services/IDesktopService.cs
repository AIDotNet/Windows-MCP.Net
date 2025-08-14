using System.Drawing;

namespace WindowsMCP.Net.Services;

/// <summary>
/// Interface for Windows desktop automation services.
/// Provides methods for interacting with Windows desktop, applications, and UI elements.
/// </summary>
public interface IDesktopService
{
    /// <summary>
    /// Launch an application from the Windows Start Menu by name.
    /// </summary>
    /// <param name="name">The name of the application to launch</param>
    /// <returns>A tuple containing the response message and status code</returns>
    Task<(string Response, int Status)> LaunchAppAsync(string name);

    /// <summary>
    /// Execute a PowerShell command and return the output.
    /// </summary>
    /// <param name="command">The PowerShell command to execute</param>
    /// <returns>A tuple containing the response and status code</returns>
    Task<(string Response, int Status)> ExecuteCommandAsync(string command);

    /// <summary>
    /// Get the current desktop state including applications and UI elements.
    /// </summary>
    /// <param name="useVision">Whether to include a screenshot in the response</param>
    /// <returns>The desktop state information</returns>
    Task<string> GetDesktopStateAsync(bool useVision = false);

    /// <summary>
    /// Copy text to clipboard or retrieve current clipboard content.
    /// </summary>
    /// <param name="mode">The mode: "copy" or "paste"</param>
    /// <param name="text">The text to copy (required for copy mode)</param>
    /// <returns>The result message</returns>
    Task<string> ClipboardOperationAsync(string mode, string? text = null);

    /// <summary>
    /// Click on UI elements at specific coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="button">Mouse button: "left", "right", or "middle"</param>
    /// <param name="clicks">Number of clicks</param>
    /// <returns>The result message</returns>
    Task<string> ClickAsync(int x, int y, string button = "left", int clicks = 1);

    /// <summary>
    /// Type text into input fields or focused elements.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="text">Text to type</param>
    /// <param name="clear">Whether to clear existing text first</param>
    /// <param name="pressEnter">Whether to press Enter after typing</param>
    /// <returns>The result message</returns>
    Task<string> TypeAsync(int x, int y, string text, bool clear = false, bool pressEnter = false);

    /// <summary>
    /// Resize or move an application window.
    /// </summary>
    /// <param name="name">The name of the application</param>
    /// <param name="width">New width (optional)</param>
    /// <param name="height">New height (optional)</param>
    /// <param name="x">New X position (optional)</param>
    /// <param name="y">New Y position (optional)</param>
    /// <returns>A tuple containing the response and status code</returns>
    Task<(string Response, int Status)> ResizeAppAsync(string name, int? width = null, int? height = null, int? x = null, int? y = null);

    /// <summary>
    /// Switch to a specific application window and bring it to foreground.
    /// </summary>
    /// <param name="name">The name of the application</param>
    /// <returns>A tuple containing the response and status code</returns>
    Task<(string Response, int Status)> SwitchAppAsync(string name);

    /// <summary>
    /// Scroll at specific coordinates or current mouse position.
    /// </summary>
    /// <param name="x">X coordinate (optional)</param>
    /// <param name="y">Y coordinate (optional)</param>
    /// <param name="type">Scroll type: "horizontal" or "vertical"</param>
    /// <param name="direction">Scroll direction: "up", "down", "left", or "right"</param>
    /// <param name="wheelTimes">Number of wheel scrolls</param>
    /// <returns>The result message</returns>
    Task<string> ScrollAsync(int? x = null, int? y = null, string type = "vertical", string direction = "down", int wheelTimes = 1);

    /// <summary>
    /// Drag and drop operation from source to destination coordinates.
    /// </summary>
    /// <param name="fromX">Source X coordinate</param>
    /// <param name="fromY">Source Y coordinate</param>
    /// <param name="toX">Destination X coordinate</param>
    /// <param name="toY">Destination Y coordinate</param>
    /// <returns>The result message</returns>
    Task<string> DragAsync(int fromX, int fromY, int toX, int toY);

    /// <summary>
    /// Move mouse cursor to specific coordinates without clicking.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>The result message</returns>
    Task<string> MoveAsync(int x, int y);

    /// <summary>
    /// Execute keyboard shortcuts using key combinations.
    /// </summary>
    /// <param name="keys">Array of keys to press simultaneously</param>
    /// <returns>The result message</returns>
    Task<string> ShortcutAsync(string[] keys);

    /// <summary>
    /// Press individual keyboard keys.
    /// </summary>
    /// <param name="key">The key to press</param>
    /// <returns>The result message</returns>
    Task<string> KeyAsync(string key);

    /// <summary>
    /// Pause execution for specified duration.
    /// </summary>
    /// <param name="duration">Duration in seconds</param>
    /// <returns>The result message</returns>
    Task<string> WaitAsync(int duration);

    /// <summary>
    /// Fetch and convert webpage content to markdown format.
    /// </summary>
    /// <param name="url">The URL to scrape</param>
    /// <returns>The scraped content in markdown format</returns>
    Task<string> ScrapeAsync(string url);

    /// <summary>
    /// Get the default language of the user interface.
    /// </summary>
    /// <returns>The default language</returns>
    string GetDefaultLanguage();
}