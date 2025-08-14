using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
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
    /// <returns>输入操作的结果描述</returns>
    public async Task<string> TypeAsync(int x, int y, string text, bool clear = false, bool pressEnter = false)
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

            SendTextInput(text);

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
    /// 根据窗口标题查找窗口句柄
    /// </summary>
    /// <param name="title">要查找的窗口标题（支持部分匹配）</param>
    /// <returns>找到的窗口句柄，如果未找到则返回IntPtr.Zero</returns>
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
    /// 释放资源，主要是释放HttpClient
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}