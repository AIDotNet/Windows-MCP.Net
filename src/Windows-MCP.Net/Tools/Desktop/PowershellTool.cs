using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Interface;

namespace Tools.Desktop;

/// <summary>
/// MCP tool for executing PowerShell commands.
/// </summary>
[McpServerToolType]
public class PowershellTool
{
    private readonly IDesktopService _desktopService;
    private readonly ILogger<PowershellTool> _logger;

    public PowershellTool(IDesktopService desktopService, ILogger<PowershellTool> logger)
    {
        _desktopService = desktopService;
        _logger = logger;
    }

    /// <summary>
    /// Execute PowerShell commands and return the output with status code.
    /// </summary>
    /// <param name="command">The PowerShell command to execute</param>
    /// <returns>Command output with status code</returns>
    [McpServerTool, Description("Execute PowerShell commands and return the output with status code")]
    public async Task<string> ExecuteCommandAsync(
        [Description("The PowerShell command to execute")] string command)
    {
        _logger.LogInformation("Executing PowerShell command: {Command}", command);
        
        var (response, status) = await _desktopService.ExecuteCommandAsync(command);
        
        return $"Status Code: {status}\nResponse: {response}";
    }
}