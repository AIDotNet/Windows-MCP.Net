using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using HtmlAgilityPack;
using ReverseMarkdown;
using Interface;

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

    [DllImport("user32.dll")]
    private static extern bool IsWindowEnabled(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(POINT Point);

    [DllImport("user32.dll")]
    private static extern IntPtr ChildWindowFromPoint(IntPtr hWndParent, POINT Point);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string? lpszClass, string? lpszWindow);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    // Clipboard API imports
    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll")]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalFree(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern UIntPtr GlobalSize(IntPtr hMem);

    // Keyboard input API imports
    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    // Screenshot API imports
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hGDIObj);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, uint dwRop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hDC);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    // DPI awareness API imports
    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    [DllImport("shcore.dll")]
    private static extern int SetProcessDpiAwareness(int value);

    // Constants for screenshot
    private const uint SRCCOPY = 0x00CC0020;
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;
    private const int SM_CXVIRTUALSCREEN = 78;
    private const int SM_CYVIRTUALSCREEN = 79;
    private const int SM_XVIRTUALSCREEN = 76;
    private const int SM_YVIRTUALSCREEN = 77;
    
    // Constants for GetDeviceCaps
    private const int LOGPIXELSX = 88;
    private const int LOGPIXELSY = 90;
    private const int DESKTOPHORZRES = 118;
    private const int DESKTOPVERTRES = 117;
    
    // DPI awareness constants
    private const int PROCESS_DPI_UNAWARE = 0;
    private const int PROCESS_SYSTEM_DPI_AWARE = 1;
    private const int PROCESS_PER_MONITOR_DPI_AWARE = 2;

    // Constants for clipboard
    private const uint CF_TEXT = 1;
    private const uint CF_UNICODETEXT = 13;
    private const uint GMEM_MOVEABLE = 0x0002;
    private const uint GMEM_ZEROINIT = 0x0040;
    private const uint GHND = GMEM_MOVEABLE | GMEM_ZEROINIT;

    // Constants for keyboard input
    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;
    private const uint KEYEVENTF_SCANCODE = 0x0008;

    // Virtual key codes
    private const ushort VK_CONTROL = 0x11;
    private const ushort VK_MENU = 0x12; // Alt key
    private const ushort VK_SHIFT = 0x10;
    private const ushort VK_RETURN = 0x0D;
    private const ushort VK_BACK = 0x08;
    private const ushort VK_DELETE = 0x2E;
    private const ushort VK_TAB = 0x09;
    private const ushort VK_ESCAPE = 0x1B;
    private const ushort VK_SPACE = 0x20;
    private const ushort VK_UP = 0x26;
    private const ushort VK_DOWN = 0x28;
    private const ushort VK_LEFT = 0x25;
    private const ushort VK_RIGHT = 0x27;

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

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint Type;
        public INPUTUNION Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUTUNION
    {
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;
        [FieldOffset(0)]
        public KEYBDINPUT Keyboard;
        [FieldOffset(0)]
        public HARDWAREINPUT Hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public uint Msg;
        public ushort ParamL;
        public ushort ParamH;
    }

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    /// <summary>
    /// 初始化桌面服务实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public DesktopService(ILogger<DesktopService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        
        // 尝试设置DPI感知以正确处理高DPI屏幕
        InitializeDpiAwareness();
    }
    
    /// <summary>
    /// 获取窗口文本的辅助方法
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>窗口文本</returns>
    private string GetWindowText(IntPtr hWnd)
    {
        const int maxLength = 256;
        var text = new StringBuilder(maxLength);
        GetWindowText(hWnd, text, maxLength);
        return text.ToString();
    }

    /// <summary>
    /// 获取窗口类名的辅助方法
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>窗口类名</returns>
    private string GetClassName(IntPtr hWnd)
    {
        const int maxLength = 256;
        var className = new StringBuilder(maxLength);
        GetClassName(hWnd, className, maxLength);
        return className.ToString();
    }

    /// <summary>
    /// 初始化DPI感知设置
    /// </summary>
    private void InitializeDpiAwareness()
    {
        try
        {
            // 尝试设置Per-Monitor DPI感知（Windows 8.1+）
            var result = SetProcessDpiAwareness(PROCESS_PER_MONITOR_DPI_AWARE);
            if (result == 0)
            {
                _logger.LogInformation("Successfully set Per-Monitor DPI awareness");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set Per-Monitor DPI awareness, trying fallback");
        }
        
        try
        {
            // 回退到系统DPI感知（Windows Vista+）
            if (SetProcessDPIAware())
            {
                _logger.LogInformation("Successfully set System DPI awareness");
            }
            else
            {
                _logger.LogWarning("Failed to set System DPI awareness");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set any DPI awareness");
        }
    }

    /// <summary>
    /// 异步启动指定名称的应用程序
    /// </summary>
    /// <param name="name">要启动的应用程序名称</param>
    /// <returns>包含响应消息和状态码的元组</returns>
    public async Task<(string Response, int Status)> LaunchAppAsync(string name)
    {
        try
        {
            // 应用程序名称映射表，支持中英文名称
            var appMappings = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "calculator", new[] { "calc.exe", "calculator", "计算器" } },
                { "calc", new[] { "calc.exe", "calculator", "计算器" } },
                { "计算器", new[] { "calc.exe", "calculator", "计算器" } },
                { "notepad", new[] { "notepad.exe", "notepad", "记事本" } },
                { "记事本", new[] { "notepad.exe", "notepad", "记事本" } },
                { "paint", new[] { "mspaint.exe", "paint", "画图" } },
                { "mspaint", new[] { "mspaint.exe", "paint", "画图" } },
                { "画图", new[] { "mspaint.exe", "paint", "画图" } },
                { "cmd", new[] { "cmd.exe", "cmd", "命令提示符" } },
                { "命令提示符", new[] { "cmd.exe", "cmd", "命令提示符" } },
                { "powershell", new[] { "powershell.exe", "powershell", "PowerShell" } },
                { "explorer", new[] { "explorer.exe", "explorer", "资源管理器" } },
                { "资源管理器", new[] { "explorer.exe", "explorer", "资源管理器" } }
            };

            // 创建超时控制
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            
            // 尝试多种启动方式
            var launchMethods = new List<Func<CancellationToken, Task<(string Response, int Status)>>>();
            
            // 方法1: 如果有映射，尝试直接启动可执行文件
            if (appMappings.TryGetValue(name, out var mappings))
            {
                var exeName = mappings[0]; // 第一个是可执行文件名
                launchMethods.Add((ct) => TryLaunchDirectly(exeName, name, ct));
            }
            
            // 方法2: 尝试使用start命令启动
            launchMethods.Add((ct) => TryLaunchWithStart(name, ct));
            
            // 方法3: 如果有映射，尝试其他名称
            if (appMappings.TryGetValue(name, out mappings))
            {
                foreach (var altName in mappings.Skip(1))
                {
                    launchMethods.Add((ct) => TryLaunchWithStart(altName, ct));
                }
            }

            // 依次尝试各种启动方法
            foreach (var method in launchMethods)
            {
                try
                {
                    var result = await method(cts.Token).WaitAsync(cts.Token);
                    if (result.Status == 0)
                    {
                        // 等待应用启动完成，但限制时间
                        await Task.Delay(1000, cts.Token);
                        
                        // 尝试找到新启动的窗口并获取坐标
                        var windowInfo = GetLaunchedWindowInfo(name);
                        if (!string.IsNullOrEmpty(windowInfo))
                        {
                            return ($"Successfully launched {name}. {windowInfo}", 0);
                        }
                        
                        return ($"Successfully launched {name}", 0);
                    }
                }
                catch (OperationCanceledException)
                {
                    return ($"Failed to launch {name}: Operation timed out", 1);
                }
            }
            
            return ($"Failed to launch {name}: All launch methods failed", 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error launching application {Name}", name);
            return ($"Error launching {name}: {ex.Message}", 1);
        }
    }
    
    /// <summary>
    /// 尝试直接启动可执行文件
    /// </summary>
    private async Task<(string Response, int Status)> TryLaunchDirectly(string exeName, string displayName, CancellationToken cancellationToken = default)
    {
        try
        {
            // 对于系统程序，使用cmd /c启动以避免弹窗
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{exeName}\" >nul 2>&1",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                // 不等待进程退出，因为GUI应用会继续运行，但添加超时控制
                await Task.Delay(1000, cancellationToken); // 给应用启动时间
                
                // 检查进程是否还在运行或已经启动了目标应用
                if (!process.HasExited || process.ExitCode == 0)
                {
                    return ($"Successfully launched {displayName} directly", 0);
                }
                else
                {
                    return ($"Failed to launch {displayName} directly", 1);
                }
            }
            return ($"Failed to launch {displayName} directly", 1);
        }
        catch (OperationCanceledException)
        {
            return ($"Failed to launch {displayName} directly: Operation timed out", 1);
        }
        catch (Exception ex)
        {
            return ($"Failed to launch {displayName} directly: {ex.Message}", 1);
        }
    }
    
    /// <summary>
    /// 尝试使用start命令启动应用
    /// </summary>
    private async Task<(string Response, int Status)> TryLaunchWithStart(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start \"\" \"{name}\" 2>nul",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                // 不等待进程退出，因为start命令会立即返回，但添加短暂延迟检查
                await Task.Delay(1000, cancellationToken);
                
                // 检查进程是否已经退出
                if (process.HasExited)
                {
                    if (process.ExitCode == 0)
                    {
                        return ($"Successfully launched {name} with start command", 0);
                    }
                    else
                    {
                        return ($"Failed to launch {name} with start command: Exit code {process.ExitCode}", 1);
                    }
                }
                else
                {
                    // 进程还在运行，可能是在等待，强制终止并返回失败
                    try { process.Kill(); } catch { }
                    return ($"Failed to launch {name} with start command: Process did not exit", 1);
                }
            }
            return ($"Failed to launch {name} with start command", 1);
        }
        catch (OperationCanceledException)
        {
            return ($"Failed to launch {name} with start command: Operation timed out", 1);
        }
        catch (Exception ex)
        {
            return ($"Failed to launch {name} with start command: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 异步执行PowerShell命令
    /// </summary>
    /// <param name="command">要执行的PowerShell命令</param>
    /// <returns>包含命令输出和退出码的元组</returns>
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

    /// <summary>
    /// 异步获取桌面状态信息，包括当前焦点窗口和所有可见窗口
    /// </summary>
    /// <param name="useVision">是否使用视觉识别（当前未实现）</param>
    /// <returns>桌面状态的详细描述</returns>
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

    /// <summary>
    /// 执行剪贴板操作（复制或粘贴）
    /// </summary>
    /// <param name="mode">操作模式："copy"（复制）或"paste"（粘贴）</param>
    /// <param name="text">要复制的文本（仅在复制模式下需要）</param>
    /// <returns>操作结果描述</returns>
    public Task<string> ClipboardOperationAsync(string mode, string? text = null)
    {
        try
        {
            if (mode.ToLower() == "copy")
            {
                if (string.IsNullOrEmpty(text))
                {
                    return Task.FromResult("No text provided to copy");
                }
                SetClipboardText(text);
                return Task.FromResult($"Copied \"{text}\" to clipboard");
            }
            else if (mode.ToLower() == "paste")
            {
                var clipboardContent = GetClipboardText();
                return Task.FromResult($"Clipboard Content: \"{clipboardContent}\"");
            }
            else
            {
                return Task.FromResult("Invalid mode. Use \"copy\" or \"paste\"");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error with clipboard operation");
            return Task.FromResult($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 在指定坐标位置执行鼠标点击操作
    /// </summary>
    /// <param name="x">点击的X坐标</param>
    /// <param name="y">点击的Y坐标</param>
    /// <param name="button">鼠标按钮类型："left"（左键）、"right"（右键）或"middle"（中键）</param>
    /// <param name="clicks">点击次数：1为单击，2为双击，3为三击</param>
    /// <returns>点击操作的结果描述</returns>
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

    /// <summary>
    /// 在指定坐标位置输入文本
    /// </summary>
    /// <param name="x">输入位置的X坐标</param>
    /// <param name="y">输入位置的Y坐标</param>
    /// <param name="text">要输入的文本内容</param>
    /// <param name="clear">是否在输入前清空现有文本</param>
    /// <param name="pressEnter">是否在输入后按回车键</param>
    /// <param name="interpretSpecialCharacters">Whether to interpret special characters as key presses</param>
    /// <returns>输入操作的结果描述</returns>
    public async Task<string> TypeAsync(int x, int y, string text, bool clear = false, bool pressEnter = false, bool interpretSpecialCharacters = false)
    {
        try
        {
            // Click at the position first
            await ClickAsync(x, y);
            await Task.Delay(200);

            if (clear)
            {
                SendKeyboardInput(VK_CONTROL, true); // Ctrl down
                SendKeyboardInput((ushort)'A', true); // A down
                SendKeyboardInput((ushort)'A', false); // A up
                SendKeyboardInput(VK_CONTROL, false); // Ctrl up
                await Task.Delay(100);
                SendKeyboardInput(VK_BACK, true); // Backspace down
                SendKeyboardInput(VK_BACK, false); // Backspace up
                await Task.Delay(100);
            }

            if (interpretSpecialCharacters)
            {
                var textToSend = new StringBuilder();
                var processedText = text.Replace("\r\n", "\n");

                foreach (char c in processedText)
                {
                    ushort vkCode = 0;
                    switch (c)
                    {
                        case '\n':
                            vkCode = VK_RETURN;
                            break;
                        case '\t':
                            vkCode = VK_TAB;
                            break;
                        case '\b':
                            vkCode = VK_BACK;
                            break;
                    }

                    if (vkCode != 0)
                    {
                        if (textToSend.Length > 0)
                        {
                            SendTextInput(textToSend.ToString());
                            textToSend.Clear();
                        }
                        SendKeyboardInput(vkCode, true);
                        SendKeyboardInput(vkCode, false);
                    }
                    else
                    {
                        textToSend.Append(c);
                    }
                }

                if (textToSend.Length > 0)
                {
                    SendTextInput(textToSend.ToString());
                }
            }
            else
            {
                SendTextInput(text);
            }

            if (pressEnter)
            {
                await Task.Delay(100);
                SendKeyboardInput(VK_RETURN, true); // Enter down
                SendKeyboardInput(VK_RETURN, false); // Enter up
            }

            return $"Typed '{text}' at ({x},{y})";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error typing text at {X},{Y}", x, y);
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// 调整指定应用程序窗口的大小和位置
    /// </summary>
    /// <param name="name">应用程序窗口名称</param>
    /// <param name="width">新的窗口宽度（可选）</param>
    /// <param name="height">新的窗口高度（可选）</param>
    /// <param name="x">新的窗口X坐标位置（可选）</param>
    /// <param name="y">新的窗口Y坐标位置（可选）</param>
    /// <returns>包含操作结果和状态码的元组</returns>
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

    /// <summary>
    /// 切换到指定名称的应用程序窗口并将其置于前台
    /// </summary>
    /// <param name="name">要切换到的应用程序窗口名称</param>
    /// <returns>包含操作结果和状态码的元组</returns>
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

    /// <summary>
    /// 获取指定应用程序窗口的位置和大小信息
    /// </summary>
    /// <param name="name">应用程序窗口名称</param>
    /// <returns>包含窗口信息和状态码的元组</returns>
    public async Task<(string Response, int Status)> GetWindowInfoAsync(string name)
    {
        try
        {
            var window = FindWindowByTitle(name);
            if (window == IntPtr.Zero)
            {
                return ($"Window '{name}' not found", 1);
            }

            GetWindowRect(window, out RECT rect);
            var windowTitle = GetWindowTitle(window);
            var width = rect.Right - rect.Left;
            var height = rect.Bottom - rect.Top;
            var centerX = (rect.Left + rect.Right) / 2;
            var centerY = (rect.Top + rect.Bottom) / 2;
            
            var info = $"Window '{windowTitle}' information:\n" +
                      $"Position: Left={rect.Left}, Top={rect.Top}, Right={rect.Right}, Bottom={rect.Bottom}\n" +
                      $"Size: Width={width}, Height={height}\n" +
                      $"Center: ({centerX},{centerY})";
            
            return (info, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting window info for {Name}", name);
            return ($"Error: {ex.Message}", 1);
        }
    }

    /// <summary>
    /// 在指定位置执行滚动操作
    /// </summary>
    /// <param name="x">滚动位置的X坐标（可选，默认使用当前鼠标位置）</param>
    /// <param name="y">滚动位置的Y坐标（可选，默认使用当前鼠标位置）</param>
    /// <param name="type">滚动类型："vertical"（垂直）或"horizontal"（水平）</param>
    /// <param name="direction">滚动方向："up"（向上）、"down"（向下）、"left"（向左）或"right"（向右）</param>
    /// <param name="wheelTimes">滚轮滚动次数</param>
    /// <returns>滚动操作的结果描述</returns>
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

    /// <summary>
    /// 执行拖拽操作，从起始坐标拖拽到目标坐标
    /// </summary>
    /// <param name="fromX">拖拽起始位置的X坐标</param>
    /// <param name="fromY">拖拽起始位置的Y坐标</param>
    /// <param name="toX">拖拽目标位置的X坐标</param>
    /// <param name="toY">拖拽目标位置的Y坐标</param>
    /// <returns>拖拽操作的结果描述</returns>
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

    /// <summary>
    /// 移动鼠标指针到指定坐标位置
    /// </summary>
    /// <param name="x">目标位置的X坐标</param>
    /// <param name="y">目标位置的Y坐标</param>
    /// <returns>移动操作的结果描述</returns>
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

    /// <summary>
    /// 执行键盘快捷键组合
    /// </summary>
    /// <param name="keys">按键组合数组，支持修饰键（ctrl、alt、shift）和普通按键</param>
    /// <returns>快捷键操作的结果描述</returns>
    public Task<string> ShortcutAsync(string[] keys)
    {
        try
        {
            var modifierKeys = new List<ushort>();
            var regularKeys = new List<ushort>();

            foreach (var key in keys)
            {
                var vk = key.ToLower() switch
                {
                    "ctrl" => VK_CONTROL,
                    "alt" => VK_MENU,
                    "shift" => VK_SHIFT,
                    _ => GetVirtualKeyCode(key)
                };

                if (key.ToLower() is "ctrl" or "alt" or "shift")
                {
                    modifierKeys.Add(vk);
                }
                else
                {
                    regularKeys.Add(vk);
                }
            }

            // Press modifier keys down
            foreach (var modKey in modifierKeys)
            {
                SendKeyboardInput(modKey, true);
            }

            // Press and release regular keys
            foreach (var regKey in regularKeys)
            {
                SendKeyboardInput(regKey, true);
                SendKeyboardInput(regKey, false);
            }

            // Release modifier keys
            foreach (var modKey in modifierKeys)
            {
                SendKeyboardInput(modKey, false);
            }

            return Task.FromResult($"Pressed {string.Join("+", keys)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending shortcut");
            return Task.FromResult($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 按下并释放指定的键盘按键
    /// </summary>
    /// <param name="key">要按下的按键名称</param>
    /// <returns>按键操作的结果描述</returns>
    public Task<string> KeyAsync(string key)
    {
        try
        {
            var vk = GetVirtualKeyCode(key);
            
            SendKeyboardInput(vk, true); // Key down
            SendKeyboardInput(vk, false); // Key up
            
            return Task.FromResult($"Pressed the key {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pressing key");
            return Task.FromResult($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 等待指定的时间（秒）
    /// </summary>
    /// <param name="duration">等待时间（秒）</param>
    /// <returns>等待操作的结果描述</returns>
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

    /// <summary>
    /// 异步抓取指定URL的网页内容并转换为Markdown格式
    /// </summary>
    /// <param name="url">要抓取的网页URL</param>
    /// <returns>转换为Markdown格式的网页内容</returns>
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

    /// <summary>
    /// 获取系统默认的用户界面语言
    /// </summary>
    /// <returns>当前系统的默认语言显示名称</returns>
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

    /// <summary>
    /// 获取指定窗口句柄的窗口标题
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>窗口标题字符串</returns>
    private string GetWindowTitle(IntPtr hWnd)
    {
        var sb = new StringBuilder(256);
        GetWindowText(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    /// <summary>
    /// 获取新启动应用的窗口信息，包括坐标和大小
    /// </summary>
    /// <param name="appName">应用名称</param>
    /// <returns>窗口详细信息</returns>
    private string GetLaunchedWindowInfo(string appName)
    {
        try
        {
            // 使用相同的多语言映射逻辑
            var titleMappings = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "calculator", new[] { "calculator", "计算器", "calc" } },
                { "calc", new[] { "calculator", "计算器", "calc" } },
                { "计算器", new[] { "calculator", "计算器", "calc" } },
                { "notepad", new[] { "notepad", "记事本", "untitled" } },
                { "记事本", new[] { "notepad", "记事本", "untitled" } },
                { "paint", new[] { "paint", "画图", "mspaint" } },
                { "mspaint", new[] { "paint", "画图", "mspaint" } },
                { "画图", new[] { "paint", "画图", "mspaint" } },
                { "cmd", new[] { "cmd", "命令提示符", "command prompt" } },
                { "命令提示符", new[] { "cmd", "命令提示符", "command prompt" } },
                { "powershell", new[] { "powershell", "windows powershell" } },
                { "explorer", new[] { "explorer", "资源管理器", "file explorer" } },
                { "资源管理器", new[] { "explorer", "资源管理器", "file explorer" } }
            };

            var foundWindow = IntPtr.Zero;
            var windowTitle = string.Empty;
            
            // 获取要搜索的标题列表
            var searchTitles = new List<string> { appName };
            if (titleMappings.TryGetValue(appName, out var mappings))
            {
                searchTitles.AddRange(mappings);
            }
            
            // 枚举所有可见窗口，查找包含应用名称的窗口
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    var title = GetWindowTitle(hWnd);
                    if (!string.IsNullOrEmpty(title) && title != "Program Manager")
                    {
                        // 检查是否匹配任何一个搜索标题
                        foreach (var searchTitle in searchTitles)
                        {
                            if (title.Contains(searchTitle, StringComparison.OrdinalIgnoreCase) ||
                                searchTitle.Contains(title, StringComparison.OrdinalIgnoreCase))
                            {
                                foundWindow = hWnd;
                                windowTitle = title;
                                return false; // 停止枚举
                            }
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);
            
            if (foundWindow != IntPtr.Zero)
            {
                GetWindowRect(foundWindow, out RECT rect);
                var width = rect.Right - rect.Left;
                var height = rect.Bottom - rect.Top;
                var centerX = (rect.Left + rect.Right) / 2;
                var centerY = (rect.Top + rect.Bottom) / 2;
                
                return $"Window '{windowTitle}' information: Position(Left={rect.Left}, Top={rect.Top}, Right={rect.Right}, Bottom={rect.Bottom}), Size(Width={width}, Height={height}), Center({centerX},{centerY})";
            }
            
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting launched window info for {AppName}", appName);
            return string.Empty;
        }
    }

    /// <summary>
    /// 根据窗口标题查找窗口句柄
    /// </summary>
    /// <param name="title">要查找的窗口标题（支持部分匹配和多语言映射）</param>
    /// <returns>找到的窗口句柄，如果未找到则返回IntPtr.Zero</returns>
    private IntPtr FindWindowByTitle(string title)
    {
        // 应用程序窗口标题映射表，支持中英文标题
        var titleMappings = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "calculator", new[] { "calculator", "计算器", "calc" } },
            { "calc", new[] { "calculator", "计算器", "calc" } },
            { "计算器", new[] { "calculator", "计算器", "calc" } },
            { "notepad", new[] { "notepad", "记事本", "untitled" } },
            { "记事本", new[] { "notepad", "记事本", "untitled" } },
            { "paint", new[] { "paint", "画图", "mspaint" } },
            { "mspaint", new[] { "paint", "画图", "mspaint" } },
            { "画图", new[] { "paint", "画图", "mspaint" } },
            { "cmd", new[] { "cmd", "命令提示符", "command prompt" } },
            { "命令提示符", new[] { "cmd", "命令提示符", "command prompt" } },
            { "powershell", new[] { "powershell", "windows powershell" } },
            { "explorer", new[] { "explorer", "资源管理器", "file explorer" } },
            { "资源管理器", new[] { "explorer", "资源管理器", "file explorer" } }
        };

        IntPtr foundWindow = IntPtr.Zero;
        
        // 获取要搜索的标题列表
        var searchTitles = new List<string> { title };
        if (titleMappings.TryGetValue(title, out var mappings))
        {
            searchTitles.AddRange(mappings);
        }
        
        EnumWindows((hWnd, lParam) =>
        {
            if (IsWindowVisible(hWnd))
            {
                var windowTitle = GetWindowTitle(hWnd);
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    // 检查是否匹配任何一个搜索标题
                    foreach (var searchTitle in searchTitles)
                    {
                        if (windowTitle.Contains(searchTitle, StringComparison.OrdinalIgnoreCase) ||
                            searchTitle.Contains(windowTitle, StringComparison.OrdinalIgnoreCase))
                        {
                            foundWindow = hWnd;
                            return false; // Stop enumeration
                        }
                    }
                }
            }
            return true;
        }, IntPtr.Zero);
        
        return foundWindow;
    }

    /// <summary>
    /// 将文本设置到系统剪贴板
    /// </summary>
    /// <param name="text">要复制到剪贴板的文本</param>
    /// <exception cref="InvalidOperationException">当无法打开剪贴板或设置数据时抛出</exception>
    /// <exception cref="OutOfMemoryException">当无法分配内存时抛出</exception>
    private void SetClipboardText(string text)
    {
        if (!OpenClipboard(IntPtr.Zero))
            throw new InvalidOperationException("Cannot open clipboard");

        try
        {
            EmptyClipboard();

            var bytes = Encoding.Unicode.GetBytes(text + "\0");
            var hMem = GlobalAlloc(GHND, (UIntPtr)bytes.Length);
            if (hMem == IntPtr.Zero)
                throw new OutOfMemoryException("Cannot allocate memory for clipboard");

            var ptr = GlobalLock(hMem);
            if (ptr == IntPtr.Zero)
            {
                GlobalFree(hMem);
                throw new InvalidOperationException("Cannot lock memory for clipboard");
            }

            try
            {
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
            }
            finally
            {
                GlobalUnlock(hMem);
            }

            if (SetClipboardData(CF_UNICODETEXT, hMem) == IntPtr.Zero)
            {
                GlobalFree(hMem);
                throw new InvalidOperationException("Cannot set clipboard data");
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    /// <summary>
    /// 从系统剪贴板获取文本内容
    /// </summary>
    /// <returns>剪贴板中的文本内容，如果为空则返回空字符串</returns>
    /// <exception cref="InvalidOperationException">当无法打开剪贴板时抛出</exception>
    private string GetClipboardText()
    {
        if (!OpenClipboard(IntPtr.Zero))
            throw new InvalidOperationException("Cannot open clipboard");

        try
        {
            var hMem = GetClipboardData(CF_UNICODETEXT);
            if (hMem == IntPtr.Zero)
                return string.Empty;

            var ptr = GlobalLock(hMem);
            if (ptr == IntPtr.Zero)
                return string.Empty;

            try
            {
                var size = (int)GlobalSize(hMem);
                var bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return Encoding.Unicode.GetString(bytes).TrimEnd('\0');
            }
            finally
            {
                GlobalUnlock(hMem);
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    /// <summary>
    /// 发送键盘输入事件
    /// </summary>
    /// <param name="virtualKey">虚拟键码</param>
    /// <param name="keyDown">true表示按下键，false表示释放键</param>
    private void SendKeyboardInput(ushort virtualKey, bool keyDown)
    {
        var input = new INPUT
        {
            Type = INPUT_KEYBOARD,
            Data = new INPUTUNION
            {
                Keyboard = new KEYBDINPUT
                {
                    VirtualKey = virtualKey,
                    ScanCode = 0,
                    Flags = keyDown ? 0 : KEYEVENTF_KEYUP,
                    Time = 0,
                    ExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
    }

    /// <summary>
    /// 发送文本输入，逐字符发送Unicode字符
    /// </summary>
    /// <param name="text">要输入的文本</param>
    private void SendTextInput(string text)
    {
        foreach (char c in text)
        {
            var input = new INPUT
            {
                Type = INPUT_KEYBOARD,
                Data = new INPUTUNION
                {
                    Keyboard = new KEYBDINPUT
                    {
                        VirtualKey = 0,
                        ScanCode = c,
                        Flags = KEYEVENTF_UNICODE,
                        Time = 0,
                        ExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());

            // Key up
            input.Data.Keyboard.Flags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;
            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }
    }

    /// <summary>
    /// 根据按键名称获取对应的虚拟键码
    /// </summary>
    /// <param name="key">按键名称（如"enter"、"escape"、"f1"等）</param>
    /// <returns>对应的虚拟键码，如果未找到则返回0</returns>
    private ushort GetVirtualKeyCode(string key)
    {
        return key.ToLower() switch
        {
            "enter" => VK_RETURN,
            "escape" => VK_ESCAPE,
            "tab" => VK_TAB,
            "space" => VK_SPACE,
            "backspace" => VK_BACK,
            "delete" => VK_DELETE,
            "up" => VK_UP,
            "down" => VK_DOWN,
            "left" => VK_LEFT,
            "right" => VK_RIGHT,
            "f1" => 0x70,
            "f2" => 0x71,
            "f3" => 0x72,
            "f4" => 0x73,
            "f5" => 0x74,
            "f6" => 0x75,
            "f7" => 0x76,
            "f8" => 0x77,
            "f9" => 0x78,
            "f10" => 0x79,
            "f11" => 0x7A,
            "f12" => 0x7B,
            _ when key.Length == 1 => (ushort)char.ToUpper(key[0]),
            _ => 0
        };
    }

    /// <summary>
    /// 异步截取屏幕并保存到临时目录
    /// </summary>
    /// <returns>保存的截图文件路径</returns>
    public async Task<string> TakeScreenshotAsync()
    {
        try
        {
            // 获取桌面窗口句柄和设备上下文
            IntPtr desktopWindow = GetDesktopWindow();
            IntPtr desktopDC = GetDC(desktopWindow);

            // 获取真实的物理屏幕尺寸（考虑DPI缩放）
            int screenWidth = GetDeviceCaps(desktopDC, DESKTOPHORZRES);
            int screenHeight = GetDeviceCaps(desktopDC, DESKTOPVERTRES);
            
            // 如果GetDeviceCaps返回0，则回退到GetSystemMetrics
            if (screenWidth == 0 || screenHeight == 0)
            {
                screenWidth = GetSystemMetrics(SM_CXSCREEN);
                screenHeight = GetSystemMetrics(SM_CYSCREEN);
                _logger.LogWarning("Using fallback screen dimensions: {Width}x{Height}", screenWidth, screenHeight);
            }
            else
            {
                _logger.LogInformation("Using DPI-aware screen dimensions: {Width}x{Height}", screenWidth, screenHeight);
            }

            // 创建兼容的设备上下文和位图
            IntPtr memoryDC = CreateCompatibleDC(desktopDC);
            IntPtr bitmap = CreateCompatibleBitmap(desktopDC, screenWidth, screenHeight);

            // 选择位图到内存设备上下文
            IntPtr oldBitmap = SelectObject(memoryDC, bitmap);

            // 复制屏幕内容到位图
            BitBlt(memoryDC, 0, 0, screenWidth, screenHeight, desktopDC, 0, 0, SRCCOPY);

            // 从位图创建Image对象
            using var image = Image.FromHbitmap(bitmap);
            
            // 生成文件名和路径
            string tempPath = Path.GetTempPath();
            string fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(tempPath, fileName);

            // 保存截图
            image.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

            // 清理资源
            SelectObject(memoryDC, oldBitmap);
            DeleteObject(bitmap);
            DeleteDC(memoryDC);
            ReleaseDC(desktopWindow, desktopDC);

            _logger.LogInformation("Screenshot saved to: {FilePath} with dimensions {Width}x{Height}", filePath, screenWidth, screenHeight);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error taking screenshot");
            throw new InvalidOperationException($"Failed to take screenshot: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 在默认浏览器中打开指定的URL
    /// </summary>
    /// <param name="url">要打开的URL，如果为空或无效则打开百度</param>
    /// <param name="searchQuery">可选的搜索词，会拼接到百度URL后面</param>
    /// <returns>操作结果消息</returns>
    public async Task<string> OpenBrowserAsync(string? url = null, string? searchQuery = null)
    {
        try
        {
            // 如果URL为空或无效，使用默认的百度URL
            if (string.IsNullOrWhiteSpace(url) || !IsValidHttpUrl(url))
            {
                url = "https://www.baidu.com";
                
                // 如果有搜索词，拼接到百度URL后面
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    var encodedQuery = Uri.EscapeDataString(searchQuery);
                    url = $"https://www.baidu.com/s?wd={encodedQuery}";
                }
            }

            _logger.LogInformation("Opening URL in browser: {Url}", url);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c start {url}",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using var process = Process.Start(processStartInfo);
            await Task.Delay(500); // 等待浏览器启动

            return $"Successfully opened {url} in default browser";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening browser with URL: {Url}", url);
            return $"Failed to open browser: {ex.Message}";
        }
    }

    /// <summary>
    /// 验证URL是否为有效的HTTP或HTTPS URL
    /// </summary>
    /// <param name="url">要验证的URL</param>
    /// <returns>如果URL有效返回true，否则返回false</returns>
    private static bool IsValidHttpUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Find UI element by text content.
    /// </summary>
    /// <param name="text">The text to search for</param>
    /// <returns>A JSON string containing element information or error message</returns>
    public async Task<string> FindElementByTextAsync(string text)
    {
        try
        {
            _logger.LogInformation("Searching for UI element with text: {Text}", text);
            
            var foundWindow = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                var windowText = GetWindowText(hWnd);
                if (!string.IsNullOrEmpty(windowText) && windowText.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    foundWindow = hWnd;
                    return false; // Stop enumeration
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);
            
            if (foundWindow != IntPtr.Zero)
            {
                GetWindowRect(foundWindow, out var rect);
                var windowText = GetWindowText(foundWindow);
                var className = GetClassName(foundWindow);
                
                var result = new
                {
                    success = true,
                    found = true,
                    element = new
                    {
                        name = windowText,
                        automationId = foundWindow.ToString(),
                        className = className,
                        controlType = "Window",
                        boundingRectangle = new
                        {
                            x = rect.Left,
                            y = rect.Top,
                            width = rect.Right - rect.Left,
                            height = rect.Bottom - rect.Top
                        },
                        isEnabled = IsWindowEnabled(foundWindow),
                        isVisible = IsWindowVisible(foundWindow)
                    }
                };
                return JsonSerializer.Serialize(result);
            }
            else
            {
                var result = new
                {
                    success = true,
                    found = false,
                    message = $"Element with text '{text}' not found"
                };
                return JsonSerializer.Serialize(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding element by text: {Text}", text);
            var result = new
            {
                success = false,
                error = ex.Message
            };
            return JsonSerializer.Serialize(result);
        }
    }

    /// <summary>
    /// Find UI element by class name.
    /// </summary>
    /// <param name="className">The class name to search for</param>
    /// <returns>A JSON string containing element information or error message</returns>
    public async Task<string> FindElementByClassNameAsync(string className)
    {
        try
        {
            _logger.LogInformation("Searching for UI element with class name: {ClassName}", className);
            
            var foundWindow = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                var windowClassName = GetClassName(hWnd);
                if (!string.IsNullOrEmpty(windowClassName) && windowClassName.Equals(className, StringComparison.OrdinalIgnoreCase))
                {
                    foundWindow = hWnd;
                    return false; // Stop enumeration
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);
            
            if (foundWindow != IntPtr.Zero)
            {
                GetWindowRect(foundWindow, out var rect);
                var windowText = GetWindowText(foundWindow);
                var windowClassName = GetClassName(foundWindow);
                
                var result = new
                {
                    success = true,
                    found = true,
                    element = new
                    {
                        name = windowText,
                        automationId = foundWindow.ToString(),
                        className = windowClassName,
                        controlType = "Window",
                        boundingRectangle = new
                        {
                            x = rect.Left,
                            y = rect.Top,
                            width = rect.Right - rect.Left,
                            height = rect.Bottom - rect.Top
                        },
                        isEnabled = IsWindowEnabled(foundWindow),
                        isVisible = IsWindowVisible(foundWindow)
                    }
                };
                return JsonSerializer.Serialize(result);
            }
            else
            {
                var result = new
                {
                    success = true,
                    found = false,
                    message = $"Element with class name '{className}' not found"
                };
                return JsonSerializer.Serialize(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding element by class name: {ClassName}", className);
            var result = new
            {
                success = false,
                error = ex.Message
            };
            return JsonSerializer.Serialize(result);
        }
    }

    /// <summary>
    /// Find UI element by automation ID.
    /// </summary>
    /// <param name="automationId">The automation ID to search for</param>
    /// <returns>A JSON string containing element information or error message</returns>
    public async Task<string> FindElementByAutomationIdAsync(string automationId)
    {
        try
        {
            _logger.LogInformation("Searching for UI element with automation ID: {AutomationId}", automationId);
            
            // For Windows API implementation, we'll try to parse the automationId as a window handle
            if (IntPtr.TryParse(automationId, out var hWnd) && hWnd != IntPtr.Zero)
            {
                if (IsWindowVisible(hWnd))
                {
                    GetWindowRect(hWnd, out var rect);
                    var windowText = GetWindowText(hWnd);
                    var windowClassName = GetClassName(hWnd);
                    
                    var result = new
                    {
                        success = true,
                        found = true,
                        element = new
                        {
                            name = windowText,
                            automationId = hWnd.ToString(),
                            className = windowClassName,
                            controlType = "Window",
                            boundingRectangle = new
                            {
                                x = rect.Left,
                                y = rect.Top,
                                width = rect.Right - rect.Left,
                                height = rect.Bottom - rect.Top
                            },
                            isEnabled = IsWindowEnabled(hWnd),
                            isVisible = IsWindowVisible(hWnd)
                        }
                    };
                    return JsonSerializer.Serialize(result);
                }
            }
            
            var notFoundResult = new
            {
                success = true,
                found = false,
                message = $"Element with automation ID '{automationId}' not found"
            };
            return JsonSerializer.Serialize(notFoundResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding element by automation ID: {AutomationId}", automationId);
            var result = new
            {
                success = false,
                error = ex.Message
            };
            return JsonSerializer.Serialize(result);
        }
    }

    /// <summary>
    /// Get properties of UI element at specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>A JSON string containing element properties or error message</returns>
    public async Task<string> GetElementPropertiesAsync(int x, int y)
    {
        try
        {
            _logger.LogInformation("Getting element properties at coordinates: ({X}, {Y})", x, y);
            
            var point = new POINT { X = x, Y = y };
            var hWnd = WindowFromPoint(point);
            
            if (hWnd != IntPtr.Zero)
            {
                GetWindowRect(hWnd, out var rect);
                var windowText = GetWindowText(hWnd);
                var windowClassName = GetClassName(hWnd);
                
                var result = new
                {
                    success = true,
                    found = true,
                    coordinates = new { x, y },
                    element = new
                    {
                        name = windowText,
                        automationId = hWnd.ToString(),
                        className = windowClassName,
                        controlType = "Window",
                        boundingRectangle = new
                        {
                            x = rect.Left,
                            y = rect.Top,
                            width = rect.Right - rect.Left,
                            height = rect.Bottom - rect.Top
                        },
                        isEnabled = IsWindowEnabled(hWnd),
                        isVisible = IsWindowVisible(hWnd),
                        hasKeyboardFocus = GetForegroundWindow() == hWnd,
                        isKeyboardFocusable = IsWindowEnabled(hWnd)
                    }
                };
                return JsonSerializer.Serialize(result);
            }
            else
            {
                var result = new
                {
                    success = true,
                    found = false,
                    coordinates = new { x, y },
                    message = $"No UI element found at coordinates ({x}, {y})"
                };
                return JsonSerializer.Serialize(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting element properties at coordinates: ({X}, {Y})", x, y);
            var result = new
            {
                success = false,
                coordinates = new { x, y },
                error = ex.Message
            };
            return JsonSerializer.Serialize(result);
        }
    }

    /// <summary>
    /// Wait for UI element to appear with specified selector.
    /// </summary>
    /// <param name="selector">The selector to wait for (text, className, or automationId)</param>
    /// <param name="selectorType">The type of selector: "text", "className", or "automationId"</param>
    /// <param name="timeout">Timeout in milliseconds</param>
    /// <returns>A JSON string containing element information or timeout message</returns>
    public async Task<string> WaitForElementAsync(string selector, string selectorType, int timeout = 5000)
    {
        try
        {
            _logger.LogInformation("Waiting for UI element with {SelectorType}: {Selector}, timeout: {Timeout}ms", selectorType, selector, timeout);
            
            var startTime = DateTime.Now;
            var timeoutSpan = TimeSpan.FromMilliseconds(timeout);
            
            while (DateTime.Now - startTime < timeoutSpan)
            {
                try
                {
                    var foundWindow = IntPtr.Zero;
                    
                    EnumWindows((hWnd, lParam) =>
                    {
                        var match = selectorType.ToLower() switch
                        {
                            "text" => GetWindowText(hWnd).Contains(selector, StringComparison.OrdinalIgnoreCase),
                            "classname" => GetClassName(hWnd).Equals(selector, StringComparison.OrdinalIgnoreCase),
                            "automationid" => IntPtr.TryParse(selector, out var targetHWnd) && hWnd == targetHWnd,
                            _ => throw new ArgumentException($"Invalid selector type: {selectorType}")
                        };
                        
                        if (match && IsWindowVisible(hWnd))
                        {
                            foundWindow = hWnd;
                            return false; // Stop enumeration
                        }
                        return true; // Continue enumeration
                    }, IntPtr.Zero);
                    
                    if (foundWindow != IntPtr.Zero)
                    {
                        GetWindowRect(foundWindow, out var rect);
                        var windowText = GetWindowText(foundWindow);
                        var windowClassName = GetClassName(foundWindow);
                        
                        var result = new
                        {
                            success = true,
                            found = true,
                            waitTime = (int)(DateTime.Now - startTime).TotalMilliseconds,
                            selector,
                            selectorType,
                            element = new
                            {
                                name = windowText,
                                automationId = foundWindow.ToString(),
                                className = windowClassName,
                                controlType = "Window",
                                boundingRectangle = new
                                {
                                    x = rect.Left,
                                    y = rect.Top,
                                    width = rect.Right - rect.Left,
                                    height = rect.Bottom - rect.Top
                                },
                                isEnabled = IsWindowEnabled(foundWindow),
                                isVisible = IsWindowVisible(foundWindow)
                            }
                        };
                        return JsonSerializer.Serialize(result);
                    }
                }
                catch (Exception innerEx)
                {
                    // Window might be in transition, continue waiting
                    _logger.LogDebug(innerEx, "Error during element search, continuing to wait");
                }
                
                await Task.Delay(100); // Wait 100ms before next attempt
            }
            
            // Timeout reached
            var timeoutResult = new
            {
                success = true,
                found = false,
                timeout = true,
                waitTime = timeout,
                selector,
                selectorType,
                message = $"Element with {selectorType} '{selector}' not found within {timeout}ms timeout"
            };
            return JsonSerializer.Serialize(timeoutResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error waiting for element with {SelectorType}: {Selector}", selectorType, selector);
            var result = new
            {
                success = false,
                selector,
                selectorType,
                error = ex.Message
            };
            return JsonSerializer.Serialize(result);
        }
    }

    /// <summary>
    /// 释放资源，主要是释放HttpClient
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}