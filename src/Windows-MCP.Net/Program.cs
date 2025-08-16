using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog;
using WindowsMCP.Net.Services;
using WindowsMCP.Net.Tools;
using System.Reflection;
using Interface;

/// <summary>
/// Main entry point for the Windows MCP Server application.
/// This server provides tools for interacting with Windows desktop through the MCP protocol.
/// </summary>
// Configure global logger with structured logging for both console and file output
Log.Logger = new LoggerConfiguration()
   .MinimumLevel.Debug()
   .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
   .Enrich.FromLogContext()
   .WriteTo.Console()
   .WriteTo.File(
       "logs/winmcplog-.txt",
       rollingInterval: RollingInterval.Day, 
       retainedFileCountLimit: 31,
       outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
   )
   .CreateLogger();
// Main application entry point with proper error handling and logging
// This try-catch ensures graceful shutdown and error logging
// The application uses the MCP (Model Context Protocol) to provide Windows desktop automation tools
// All communication is done via stdio to comply with MCP protocol requirements
// Services are registered as singletons to maintain state across requests
// Tools are registered to handle Windows desktop interactions
// The server runs until explicitly terminated or an error occurs
try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
    builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

    // Add the MCP services: the transport to use (stdio) and the tools to register.
    builder.Services
        .AddSingleton<IDesktopService, DesktopService>()
        .AddSingleton<IFileSystemService, FileSystemService>()
        .AddSingleton<IOcrService, OcrService>()
        .AddSingleton<ISystemControlService, SystemControlService>()
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(Assembly.GetExecutingAssembly())
        ;

    await builder.Build().RunAsync();
}
// Global exception handler for unhandled errors
// Logs critical failures and ensures proper cleanup
// This ensures the server exits gracefully with proper error logging
catch (Exception ex)
{
    Log.Fatal(ex, "Error");
}