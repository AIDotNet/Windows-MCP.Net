using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using HtmlAgilityPack;
using ReverseMarkdown;

namespace WindowsMCP.Net.Services;

/// <summary>
/// Implementation of Windows desktop automation services.
/// Provides methods for interacting with Windows desktop, applications, and UI elements.
/// </summary>
public class DesktopService : IDesktopService
{
    private readonly ILogger<DesktopService> _logger;
    private readonly HttpClient _httpClient;

    // Windows API imports
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    // Constants for mouse events
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const uint MOUSEEVENTF_WHEEL = 0x0800;

    // Constants for window positioning
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_SHOWWINDOW = 0x0040;

    // Window show states
    private const int SW_RESTORE = 9;
    private const int SW_MAXIMIZE = 3;
    private const int SW_MINIMIZE = 6;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public DesktopService(ILogger<DesktopService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public async Task<(string Response, int Status)> LaunchAppAsync(string name)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start {name}",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                if (process.ExitCode == 0)
                {
                    return ($"Successfully launched {name}", 0);
                }
            }
            return ($"Failed to launch {name}", 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error launching application {Name}", name);
            return ($"Error launching {name}: {ex.Message}", 1);
        }
    }

    public async Task<(string Response, int Status)> ExecuteCommandAsync(string command)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                var result = string.IsNullOrEmpty(error) ? output : $"{output}\nError: {error}";
                return (result.Trim(), process.ExitCode);
            }
            return ("Failed to start PowerShell process", 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command {Command}", command);
            return ($"Error: {ex.Message}", 1);
        }
    }

    public async Task<string> GetDesktopStateAsync(bool useVision = false)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Default Language of User Interface:");
            sb.AppendLine(GetDefaultLanguage());
            sb.AppendLine();

            // Get focused window
            var foregroundWindow = GetForegroundWindow();
            if (foregroundWindow != IntPtr.Zero)
            {
                var windowTitle = GetWindowTitle(foregroundWindow);
                sb.AppendLine($"Focused App:");
                sb.AppendLine(windowTitle);
                sb.AppendLine();
            }

            // Get all visible windows
            var windows = new List<string>();
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    var title = GetWindowTitle(hWnd);
                    if (!string.IsNullOrEmpty(title) && title != "Program Manager")
                    {
                        windows.Add(title);
                    }
                }
                return true;
            }, IntPtr.Zero);

            sb.AppendLine($"Opened Apps:");
            foreach (var window in windows.Take(10)) // Limit to first 10 windows
            {
                sb.AppendLine($"- {window}");
            }
            sb.AppendLine();

            sb.AppendLine("List of Interactive Elements:");
            sb.AppendLine("Desktop elements available for interaction.");
            sb.AppendLine();

            sb.AppendLine("List of Informative Elements:");
            sb.AppendLine("Current desktop information displayed.");
            sb.AppendLine();

            sb.AppendLine("List of Scrollable Elements:");
            sb.AppendLine("Areas that can be scrolled.");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting desktop state");
            return $"Error getting desktop state: {ex.Message}";
        }
    }

    public async Task<string> ClipboardOperationAsync(string mode, string? text = null)
    {
        try
        {
            if (mode.ToLower() == "copy")
            {
                if (string.IsNullOrEmpty(text))
                {
                    return "No text provided to copy";
                }
                Clipboard.SetText(text);
                return $"Copied \"{text}\" to clipboard";
            }
            else if (mode.ToLower() == "paste")
            {
                var clipboardContent = Clipboard.GetText();
                return $"Clipboard Content: \"{clipboardContent}\"";
            }
            else
            {
                return "Invalid mode. Use \"copy\" or \"paste\"";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error with clipboard operation");
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> ClickAsync(int x, int y, string button = "left", int clicks = 1)
    {
        try
        {
            SetCursorPos(x, y);
            await Task.Delay(100); // Small delay for cursor positioning

            uint downFlag, upFlag;
            switch (button.ToLower())
            {
                case "right":
                    downFlag = MOUSEEVENTF_RIGHTDOWN;
                    upFlag = MOUSEEVENTF_RIGHTUP;
                    break;
                case "middle":
                    downFlag = MOUSEEVENTF_MIDDLEDOWN;
                    upFlag = MOUSEEVENTF_MIDDLEUP;
                    break;
                default:
                    downFlag = MOUSEEVENTF_LEFTDOWN;
                    upFlag = MOUSEEVENTF_LEFTUP;
                    break;
            }

            for (int i = 0; i < clicks; i++)
            {
                mouse_event(downFlag, 0, 0, 0, UIntPtr.Zero);
                await Task.Delay(50);
                mouse_event(upFlag, 0, 0, 0, UIntPtr.Zero);
                if (i < clicks - 1) await Task.Delay(100);
            }

            var clickType = clicks == 1 ? "Single" : clicks == 2 ? "Double" : "Triple";
            return $"{clickType} {button} clicked at ({x},{y})";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clicking at {X},{Y}", x, y);
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> TypeAsync(int x, int y, string text, bool clear = false, bool pressEnter = false)
    {
        try
        {
            // Click at the position first
            await ClickAsync(x, y);
            await Task.Delay(200);

            if (clear)
            {
                SendKeys.SendWait("^a"); // Ctrl+A
                await Task.Delay(100);
                SendKeys.SendWait("{BACKSPACE}");
                await Task.Delay(100);
            }

            SendKeys.SendWait(text);

            if (pressEnter)
            {
                await Task.Delay(100);
                SendKeys.SendWait("{ENTER}");
            }

            return $"Typed '{text}' at ({x},{y})";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error typing text at {X},{Y}", x, y);
            return $"Error: {ex.Message}";
        }
    }

    public async Task<(string Response, int Status)> ResizeAppAsync(string name, int? width = null, int? height = null, int? x = null, int? y = null)
    {
        try
        {
            var window = FindWindowByTitle(name);
            if (window == IntPtr.Zero)
            {
                return ($"Window '{name}' not found", 1);
            }

            GetWindowRect(window, out RECT rect);
            
            var newX = x ?? rect.Left;
            var newY = y ?? rect.Top;
            var newWidth = width ?? (rect.Right - rect.Left);
            var newHeight = height ?? (rect.Bottom - rect.Top);

            var flags = SWP_NOZORDER | SWP_SHOWWINDOW;
            if (x == null && y == null) flags |= SWP_NOMOVE;
            if (width == null && height == null) flags |= SWP_NOSIZE;

            SetWindowPos(window, IntPtr.Zero, newX, newY, newWidth, newHeight, flags);
            
            return ($"Successfully resized/moved '{name}' window", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing window {Name}", name);
            return ($"Error: {ex.Message}", 1);
        }
    }

    public async Task<(string Response, int Status)> SwitchAppAsync(string name)
    {
        try
        {
            var window = FindWindowByTitle(name);
            if (window == IntPtr.Zero)
            {
                return ($"Window '{name}' not found", 1);
            }

            ShowWindow(window, SW_RESTORE);
            SetForegroundWindow(window);
            
            return ($"Successfully switched to '{name}' window", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error switching to window {Name}", name);
            return ($"Error: {ex.Message}", 1);
        }
    }

    public async Task<string> ScrollAsync(int? x = null, int? y = null, string type = "vertical", string direction = "down", int wheelTimes = 1)
    {
        try
        {
            if (x.HasValue && y.HasValue)
            {
                SetCursorPos(x.Value, y.Value);
                await Task.Delay(100);
            }

            int delta = direction.ToLower() == "up" || direction.ToLower() == "left" ? 120 : -120;
            
            for (int i = 0; i < wheelTimes; i++)
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)delta, UIntPtr.Zero);
                await Task.Delay(100);
            }

            return $"Scrolled {type} {direction} by {wheelTimes} wheel times";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scrolling");
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> DragAsync(int fromX, int fromY, int toX, int toY)
    {
        try
        {
            SetCursorPos(fromX, fromY);
            await Task.Delay(100);
            
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(100);
            
            SetCursorPos(toX, toY);
            await Task.Delay(500); // Drag duration
            
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            
            return $"Dragged from ({fromX},{fromY}) to ({toX},{toY})";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dragging");
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> MoveAsync(int x, int y)
    {
        try
        {
            SetCursorPos(x, y);
            return $"Moved mouse pointer to ({x},{y})";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving mouse");
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> ShortcutAsync(string[] keys)
    {
        try
        {
            var keyString = string.Join("+", keys.Select(k => k.ToLower() switch
            {
                "ctrl" => "^",
                "alt" => "%",
                "shift" => "+",
                "win" => "^", // Windows key approximation
                _ => $"{{{k.ToUpper()}}}"
            }));
            
            SendKeys.SendWait(keyString);
            return $"Pressed {string.Join("+", keys)}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending shortcut");
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> KeyAsync(string key)
    {
        try
        {
            var keyCode = key.ToLower() switch
            {
                "enter" => "{ENTER}",
                "escape" => "{ESC}",
                "tab" => "{TAB}",
                "space" => " ",
                "backspace" => "{BACKSPACE}",
                "delete" => "{DELETE}",
                "up" => "{UP}",
                "down" => "{DOWN}",
                "left" => "{LEFT}",
                "right" => "{RIGHT}",
                _ when key.StartsWith("f") && int.TryParse(key.Substring(1), out var fNum) && fNum >= 1 && fNum <= 12 => $"{{{key.ToUpper()}}}",
                _ => key
            };
            
            SendKeys.SendWait(keyCode);
            return $"Pressed the key {key}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pressing key");
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> WaitAsync(int duration)
    {
        try
        {
            await Task.Delay(duration * 1000);
            return $"Waited for {duration} seconds";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error waiting");
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> ScrapeAsync(string url)
    {
        try
        {
            var html = await _httpClient.GetStringAsync(url);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            
            var converter = new Converter();
            var markdown = converter.Convert(html);
            
            return $"Scraped the contents of the entire webpage:\n{markdown}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping URL {Url}", url);
            return $"Error: {ex.Message}";
        }
    }

    public string GetDefaultLanguage()
    {
        try
        {
            return CultureInfo.CurrentUICulture.DisplayName;
        }
        catch
        {
            return "English (United States)";
        }
    }

    private string GetWindowTitle(IntPtr hWnd)
    {
        var sb = new StringBuilder(256);
        GetWindowText(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private IntPtr FindWindowByTitle(string title)
    {
        IntPtr foundWindow = IntPtr.Zero;
        EnumWindows((hWnd, lParam) =>
        {
            if (IsWindowVisible(hWnd))
            {
                var windowTitle = GetWindowTitle(hWnd);
                if (windowTitle.Contains(title, StringComparison.OrdinalIgnoreCase))
                {
                    foundWindow = hWnd;
                    return false; // Stop enumeration
                }
            }
            return true;
        }, IntPtr.Zero);
        
        return foundWindow;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}