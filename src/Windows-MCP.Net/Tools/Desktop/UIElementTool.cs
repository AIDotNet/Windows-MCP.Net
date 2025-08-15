using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using WindowsMCP.Net.Services;
using System.ComponentModel;

namespace Tools.Desktop;

/// <summary>
/// MCP tool for UI element identification and interaction.
/// Provides methods to find UI elements by various selectors and get element properties.
/// </summary>
[McpServerToolType]
public class UIElementTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<UIElementTool> _logger;

    public UIElementTool(IDesktopService desktopService, ILogger<UIElementTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Find UI element by text content.
    /// </summary>
    /// <param name="text">The text to search for</param>
    /// <returns>JSON string containing element information or error message</returns>
    [McpServerTool, Description("Find UI element by text content")]
    public async Task<string> FindElementByTextAsync(
        [Description("The text to search for")] string text)
    {
        _logger.LogInformation("Finding UI element by text: {Text}", text);
        return await _desktopService.FindElementByTextAsync(text);
    }

    /// <summary>
    /// Find UI element by class name.
    /// </summary>
    /// <param name="className">The class name to search for</param>
    /// <returns>JSON string containing element information or error message</returns>
    [McpServerTool, Description("Find UI element by class name")]
    public async Task<string> FindElementByClassNameAsync(
        [Description("The class name to search for")] string className)
    {
        _logger.LogInformation("Finding UI element by class name: {ClassName}", className);
        return await _desktopService.FindElementByClassNameAsync(className);
    }

    /// <summary>
    /// Find UI element by automation ID.
    /// </summary>
    /// <param name="automationId">The automation ID to search for</param>
    /// <returns>JSON string containing element information or error message</returns>
    [McpServerTool, Description("Find UI element by automation ID")]
    public async Task<string> FindElementByAutomationIdAsync(
        [Description("The automation ID to search for")] string automationId)
    {
        _logger.LogInformation("Finding UI element by automation ID: {AutomationId}", automationId);
        return await _desktopService.FindElementByAutomationIdAsync(automationId);
    }

    /// <summary>
    /// Get properties of UI element at specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>JSON string containing element properties or error message</returns>
    [McpServerTool, Description("Get properties of UI element at specified coordinates")]
    public async Task<string> GetElementPropertiesAsync(
        [Description("X coordinate")] int x,
        [Description("Y coordinate")] int y)
    {
        _logger.LogInformation("Getting element properties at coordinates: ({X}, {Y})", x, y);
        return await _desktopService.GetElementPropertiesAsync(x, y);
    }

    /// <summary>
    /// Wait for UI element to appear with specified selector.
    /// </summary>
    /// <param name="selector">The selector to wait for (text, className, or automationId)</param>
    /// <param name="selectorType">The type of selector: "text", "className", or "automationId"</param>
    /// <param name="timeout">Timeout in milliseconds</param>
    /// <returns>JSON string containing element information or timeout message</returns>
    [McpServerTool, Description("Wait for UI element to appear with specified selector")]
    public async Task<string> WaitForElementAsync(
        [Description("The selector to wait for (text, className, or automationId)")] string selector,
        [Description("The type of selector: \"text\", \"className\", or \"automationId\"")] string selectorType,
        [Description("Timeout in milliseconds")] int timeout = 5000)
    {
        _logger.LogInformation("Waiting for UI element with {SelectorType}: {Selector}, timeout: {Timeout}ms", selectorType, selector, timeout);
        return await _desktopService.WaitForElementAsync(selector, selectorType, timeout);
    }
}