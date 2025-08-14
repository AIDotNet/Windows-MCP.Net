# Windows API 替代方案说明

## 概述

已成功移除对 `System.Windows.Forms` 的依赖，使用纯 Windows API 实现相同功能。

## 替代的功能

### 1. 剪贴板操作

**原来使用：**
```csharp
Clipboard.SetText(text);
var content = Clipboard.GetText();
```

**现在使用：**
```csharp
SetClipboardText(text);  // 使用 Windows API
var content = GetClipboardText();  // 使用 Windows API
```

**实现的 Windows API：**
- `OpenClipboard`
- `CloseClipboard`
- `EmptyClipboard`
- `SetClipboardData`
- `GetClipboardData`
- `GlobalAlloc`/`GlobalLock`/`GlobalUnlock`/`GlobalFree`

### 2. 键盘输入

**原来使用：**
```csharp
SendKeys.SendWait("^a");  // Ctrl+A
SendKeys.SendWait("{ENTER}");  // Enter键
SendKeys.SendWait("Hello");  // 文本输入
```

**现在使用：**
```csharp
SendKeyboardInput(VK_CONTROL, true);  // 按下Ctrl
SendKeyboardInput((ushort)'A', true);  // 按下A
SendKeyboardInput((ushort)'A', false); // 释放A
SendKeyboardInput(VK_CONTROL, false); // 释放Ctrl

SendKeyboardInput(VK_RETURN, true);   // 按下Enter
SendKeyboardInput(VK_RETURN, false);  // 释放Enter

SendTextInput("Hello");  // 文本输入
```

**实现的 Windows API：**
- `SendInput` - 发送键盘输入
- `VkKeyScan` - 获取字符的虚拟键码
- `MapVirtualKey` - 映射虚拟键码

## 优势

1. **无依赖性**：不再依赖 `System.Windows.Forms`
2. **更轻量**：减少了程序集引用
3. **更精确控制**：直接使用 Windows API 提供更精确的控制
4. **跨平台兼容性**：为将来可能的跨平台支持做准备
5. **性能提升**：直接调用系统API，减少中间层开销

## 支持的功能

### 剪贴板操作
- ✅ 复制文本到剪贴板
- ✅ 从剪贴板读取文本
- ✅ Unicode 文本支持

### 键盘输入
- ✅ 单个按键输入
- ✅ 组合键（Ctrl+A, Alt+Tab等）
- ✅ 功能键（F1-F12）
- ✅ 特殊键（Enter, Escape, Tab等）
- ✅ 文本输入（支持Unicode）

## 注意事项

1. **权限要求**：某些操作可能需要管理员权限
2. **线程安全**：剪贴板操作需要在UI线程中执行
3. **错误处理**：已添加适当的异常处理
4. **内存管理**：正确管理全局内存分配和释放

## 编译状态

✅ 编译成功，无错误
⚠️ 有少量警告（关于async方法，不影响功能）

这个替代方案提供了与原来相同的功能，同时消除了对 `System.Windows.Forms` 的依赖。