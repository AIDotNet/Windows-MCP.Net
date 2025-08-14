# Windows MCP.Net

一个基于 .NET 的 Windows 桌面自动化 MCP (Model Context Protocol) 服务器，为 AI 助手提供与 Windows 桌面环境交互的能力。

## 🚀 功能特性

### 核心功能
- **应用程序启动**: 通过名称从开始菜单启动应用程序
- **PowerShell 集成**: 执行 PowerShell 命令并返回结果
- **桌面状态捕获**: 获取当前桌面状态，包括活动应用、UI 元素等
- **剪贴板操作**: 复制和粘贴文本内容
- **鼠标操作**: 点击、拖拽、移动鼠标光标
- **键盘操作**: 文本输入、按键操作、快捷键组合
- **窗口管理**: 调整窗口大小、位置，切换应用程序
- **滚动操作**: 在指定坐标进行滚动操作
- **网页抓取**: 获取网页内容并转换为 Markdown 格式
- **等待控制**: 在操作间添加延迟

### 支持的工具

| 工具名称 | 功能描述 |
|---------|----------|
| **LaunchTool** | 启动应用程序 |
| **PowershellTool** | 执行 PowerShell 命令 |
| **StateTool** | 捕获桌面状态信息 |
| **ClipboardTool** | 剪贴板操作 |
| **ClickTool** | 鼠标点击操作 |
| **TypeTool** | 文本输入 |
| **ResizeTool** | 窗口大小调整 |
| **SwitchTool** | 应用程序切换 |
| **ScrollTool** | 滚动操作 |
| **DragTool** | 拖拽操作 |
| **MoveTool** | 鼠标移动 |
| **ShortcutTool** | 快捷键操作 |
| **KeyTool** | 按键操作 |
| **WaitTool** | 等待延迟 |
| **ScrapeTool** | 网页内容抓取 |

## 🛠️ 技术栈

- **.NET 10.0**: 基于最新的 .NET 框架
- **Model Context Protocol**: 使用 MCP 协议进行通信
- **Windows Forms**: 用于 Windows 桌面交互
- **Serilog**: 结构化日志记录
- **HtmlAgilityPack**: HTML 解析
- **ReverseMarkdown**: HTML 到 Markdown 转换

## 📦 安装

### 前置要求
- Windows 操作系统
- .NET 10.0 Runtime 或更高版本

### 从源码构建

```bash
# 克隆仓库
git clone https://github.com/xuzeyu91/Windows-MCP.Net.git
cd Windows-MCP.Net/src

# 构建项目
dotnet build

# 运行项目
dotnet run
```

### NuGet 包安装

```bash
dotnet tool install --global WindowsMCP.Net
```

## 🚀 使用方法

### 作为 MCP 服务器运行

```bash
# 直接运行
dotnet run --project src/Windows-MCP.Net.csproj

# 或者使用已安装的工具
windows-mcp-net
```

### MCP 客户端配置

在您的 MCP 客户端配置中添加以下配置：

#### 使用全局安装的工具（推荐）
```json
{
  "mcpServers": {
    "WindowsMCP.Net": {
      "type": "stdio",
      "command": "Windows-MCP.Net",
      "args": [],
      "env": {}
    }
  }
}
```

#### 使用dotnet run
```json
{
  "mcpServers": {
    "WindowsMCP.Net": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "path/to/Windows-MCP.Net/src/Windows-MCP.Net.csproj"],
      "env": {}
    }
  }
}
```

## 📖 API 文档

### 示例操作

#### 启动应用程序
```json
{
  "tool": "LaunchTool",
  "parameters": {
    "name": "notepad"
  }
}
```

#### 点击操作
```json
{
  "tool": "ClickTool",
  "parameters": {
    "x": 100,
    "y": 200,
    "button": "left",
    "clicks": 1
  }
}
```

#### 文本输入
```json
{
  "tool": "TypeTool",
  "parameters": {
    "text": "Hello, World!",
    "clear": false
  }
}
```

#### 获取桌面状态
```json
{
  "tool": "StateTool",
  "parameters": {
    "use_vision": false
  }
}
```

## 🏗️ 项目结构

```
src/
├── .mcp/                 # MCP 服务器配置
├── Exceptions/           # 自定义异常类
├── Models/              # 数据模型
├── Prompts/             # 提示模板
├── Services/            # 核心服务
│   ├── DesktopService.cs    # 桌面操作服务实现
│   └── IDesktopService.cs   # 桌面服务接口
├── Tools/               # MCP 工具实现
│   ├── ClickTool.cs         # 点击工具
│   ├── LaunchTool.cs        # 启动工具
│   ├── TypeTool.cs          # 输入工具
│   └── ...                  # 其他工具
├── Program.cs           # 程序入口点
├── Windows-MCP.Net.csproj   # 项目文件
└── Windows-MCP.Net.sln      # 解决方案文件
```

## 🔧 配置

### 日志配置

项目使用 Serilog 进行日志记录，日志文件保存在 `logs/` 目录下：

- 控制台输出：实时日志显示
- 文件输出：按天滚动，保留 31 天
- 日志级别：Debug 及以上

### 环境变量

| 变量名 | 描述 | 默认值 |
|--------|------|--------|
| `ASPNETCORE_ENVIRONMENT` | 运行环境 | `Production` |

## 🤝 贡献

欢迎贡献代码！请遵循以下步骤：

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📝 许可证

本项目基于 MIT 许可证开源。详情请参阅 [LICENSE](LICENSE) 文件。

## 🔗 相关链接

- [Model Context Protocol](https://modelcontextprotocol.io/)
- [.NET 文档](https://docs.microsoft.com/dotnet/)
- [Windows API 文档](https://docs.microsoft.com/windows/win32/)

## 📞 支持

如果您遇到问题或有建议，请：

1. 查看 [Issues](https://github.com/xuzeyu91/Windows-MCP.Net/issues)
2. 创建新的 Issue
3. 参与讨论

---

**注意**: 本工具需要适当的 Windows 权限来执行桌面自动化操作。请确保在受信任的环境中使用。