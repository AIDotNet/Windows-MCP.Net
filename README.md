# Windows MCP.Net

[English](README.en.md) | **中文**

一个基于 .NET 的 Windows 桌面自动化 MCP (Model Context Protocol) 服务器，为 AI 助手提供与 Windows 桌面环境交互的能力。

## 📋 目录

- [功能特性](#-功能特性)
- [使用场景](#-使用场景)
- [演示截图](#-演示截图)
- [技术栈](#️-技术栈)
- [API文档](#-api-文档)
- [项目结构](#️-项目结构)
- [功能扩展建议](#-功能扩展建议)
- [配置](#-配置)
- [贡献指南](#-贡献指南)
- [更新日志](#-更新日志)
- [支持](#-支持)

## 🚀 快速开始

### 前置要求
- Windows 操作系统
- .NET 10.0 Runtime 或更高版本

**重要提示**: 本项目需要 .NET 10 才能运行，请先确保你的本地安装了 .NET 10。如果尚未安装，请访问 [.NET 10 下载页面](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0) 进行下载和安装。

### 1. MCP 客户端配置

在您的 MCP 客户端配置中添加以下配置：

#### 使用全局安装的工具（推荐）
```json
{
    "mcpServers": {
     "WindowsMCP.Net": {
      "type": "stdio",
      "command": "dnx",
      "args": ["WindowsMCP.Net@", "--yes"],
      "env": {}
    }
    }
}
```

#### 使用项目源码直接运行（开发模式）

**方式一：工作区配置**

在项目根目录创建 `.vscode/mcp.json` 文件：
```json
{
  "mcpServers": {
    "Windows-MCP.Net-Dev": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "src/Windows-MCP.Net.csproj"],
      "cwd": "${workspaceFolder}",
      "env": {}
    }
  }
}
```

**方式二：用户配置**

通过VS Code命令面板运行 `MCP: Open User Configuration`，添加：
```json
{
  "mcpServers": {
    "Windows-MCP.Net-Local": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "src/Windows-MCP.Net.csproj"],
      "env": {}
    }
  }
}
```

> **注意**: 使用项目源码方式便于开发调试，修改代码后无需重新安装即可生效。VS Code 1.102+ 版本支持MCP服务器的自动发现和管理。

### 2. 安装和运行

#### 方式一：全局安装（推荐）
```bash
dotnet tool install --global WindowsMCP.Net
```

#### 方式二：从源码运行
```bash
# 克隆仓库
git clone https://github.com/AIDotNet/Windows-MCP.Net.git
cd Windows-MCP.Net

# 构建项目
dotnet build

# 运行项目
dotnet run --project src/Windows-MCP.Net.csproj
```

### 3. 开始使用
配置完成后，重启您的MCP客户端，即可开始使用Windows桌面自动化功能！

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
- **浏览器操作**: 在默认浏览器中打开指定URL
- **截图功能**: 截取屏幕并保存到临时目录
- **文件系统操作**: 文件和目录的创建、读取、写入、复制、移动、删除等操作
- **OCR文字识别**: 从屏幕或指定区域提取文字，查找文字位置
- **系统控制**: 调节屏幕亮度、系统音量、屏幕分辨率等系统设置
- **等待控制**: 在操作间添加延迟

### 支持的工具

## Desktop 桌面操作工具

| 工具名称 | 功能描述 |
|---------|----------|
| **LaunchTool** | 从开始菜单启动应用程序 |
| **PowershellTool** | 执行 PowerShell 命令并返回状态码 |
| **StateTool** | 捕获桌面状态信息，包括应用程序和UI元素 |
| **ClipboardTool** | 剪贴板复制和粘贴操作 |
| **ClickTool** | 鼠标点击操作（支持左键、右键、中键，单击、双击、三击） |
| **TypeTool** | 在指定坐标输入文本，支持清除和回车 |
| **ResizeTool** | 调整窗口大小和位置 |
| **SwitchTool** | 切换到指定应用程序窗口 |
| **ScrollTool** | 在指定坐标或当前鼠标位置滚动 |
| **DragTool** | 从源坐标拖拽到目标坐标 |
| **MoveTool** | 移动鼠标光标到指定坐标 |
| **ShortcutTool** | 执行键盘快捷键组合 |
| **KeyTool** | 按下单个键盘按键 |
| **WaitTool** | 暂停执行指定秒数 |
| **ScrapeTool** | 抓取网页内容并转换为Markdown格式 |
| **ScreenshotTool** | 截取屏幕并保存到临时目录，返回图片路径 |
| **OpenBrowserTool** | 在默认浏览器中打开指定URL |

## FileSystem 文件系统工具

| 工具名称 | 功能描述 |
|---------|----------|
| **ReadFileTool** | 读取指定文件的内容 |
| **WriteFileTool** | 向文件写入内容 |
| **CreateFileTool** | 创建新文件并写入指定内容 |
| **CopyFileTool** | 复制文件到指定位置 |
| **MoveFileTool** | 移动或重命名文件 |
| **DeleteFileTool** | 删除指定文件 |
| **GetFileInfoTool** | 获取文件信息（大小、创建时间等） |
| **ListDirectoryTool** | 列出目录中的文件和子目录 |
| **CreateDirectoryTool** | 创建新目录 |
| **DeleteDirectoryTool** | 删除目录及其内容 |
| **SearchFilesTool** | 在指定目录中搜索文件 |

## OCR 图像识别工具

| 工具名称 | 功能描述 |
|---------|----------|
| **ExtractTextFromScreenTool** | 使用OCR从整个屏幕提取文字 |
| **ExtractTextFromRegionTool** | 使用OCR从屏幕指定区域提取文字 |
| **FindTextOnScreenTool** | 使用OCR在屏幕上查找指定文字 |
| **GetTextCoordinatesTool** | 获取屏幕上文字的坐标位置 |
| **ExtractTextFromFileTool** | 使用OCR从图像文件中提取文字 |

## UI元素识别工具

| 工具名称 | 功能描述 |
|---------|----------|
| **FindElementByTextTool** | 通过文本内容查找UI元素 |
| **FindElementByClassNameTool** | 通过类名查找UI元素 |
| **FindElementByAutomationIdTool** | 通过自动化ID查找UI元素 |
| **GetElementPropertiesTool** | 获取指定坐标元素的属性信息 |
| **WaitForElementTool** | 等待指定元素出现在界面上 |

## SystemControl 系统控制工具

| 工具名称 | 功能描述 |
|---------|----------|
| **BrightnessTool** | 调节屏幕亮度，支持增减和设置具体百分比 |
| **VolumeTool** | 调节系统音量，支持增减和设置具体百分比 |
| **ResolutionTool** | 设置屏幕分辨率（高、中、低三档） |

## 💡 使用场景

### 🤖 AI助手桌面自动化
- **智能客服机器人**: AI助手可以自动操作Windows应用程序，帮助用户完成复杂的桌面任务
- **语音助手集成**: 结合语音识别，通过语音指令控制桌面应用程序
- **智能办公助手**: AI助手自动处理日常办公任务，如文档整理、邮件发送等

### 📊 办公自动化
- **数据录入自动化**: 自动从网页或文档中提取数据并录入到Excel或其他应用程序
- **报告生成**: 自动收集系统信息、截图，生成格式化的报告文档
- **批量文件处理**: 自动整理、重命名、分类大量文件和文档
- **邮件自动化**: 自动发送定期报告、通知邮件

### 🧪 软件测试与质量保证
- **UI自动化测试**: 模拟用户操作，自动测试桌面应用程序的功能
- **回归测试**: 自动执行重复性测试用例，确保软件质量
- **性能监控**: 自动收集应用程序性能数据，生成监控报告
- **Bug复现**: 自动重现用户报告的问题，辅助开发人员调试

### 🎯 业务流程自动化
- **客户服务**: 自动处理客户请求，更新CRM系统
- **订单处理**: 自动从多个渠道收集订单信息并录入系统
- **库存管理**: 自动更新库存数据，生成补货提醒
- **财务对账**: 自动对比不同系统的财务数据，标记差异

### 🔍 数据采集与分析
- **网页数据抓取**: 自动从多个网站收集产品价格、新闻等信息
- **竞品分析**: 定期收集竞争对手的产品信息和价格数据
- **市场调研**: 自动收集和整理市场数据，生成分析报告
- **社交媒体监控**: 监控品牌提及，自动收集用户反馈

### 🎮 游戏与娱乐
- **游戏辅助**: 自动执行重复性游戏任务（请遵守游戏规则）
- **直播助手**: 自动管理直播软件，切换场景，发送消息
- **媒体管理**: 自动整理音乐、视频文件，更新媒体库

### 🏥 医疗与健康
- **病历录入**: 自动将纸质病历转换为电子格式
- **医疗图像分析**: 结合OCR技术，自动提取医疗报告中的关键信息
- **预约管理**: 自动处理患者预约请求，更新医院管理系统

### 🏫 教育与培训
- **在线考试**: 自动批改选择题，生成成绩报告
- **课程管理**: 自动更新课程信息，发送通知给学生
- **学习进度跟踪**: 自动记录学生的学习活动，生成进度报告

### 🏭 制造业与物流
- **生产数据采集**: 自动从生产设备收集数据，更新ERP系统
- **质量检测**: 结合图像识别，自动检测产品质量
- **物流跟踪**: 自动更新货物状态，发送跟踪信息给客户

### 🔧 系统运维
- **服务器监控**: 自动检查服务器状态，生成监控报告
- **日志分析**: 自动分析系统日志，识别异常模式
- **备份管理**: 自动执行数据备份，验证备份完整性
- **软件部署**: 自动化软件安装和配置流程

## 📸 演示截图

### 文本输入演示
通过 TypeTool 在记事本中自动输入文本：

![文本输入演示](assets/NotepadWriting.png)

### 网页搜索演示
使用 ScrapeTool 打开并搜索网页内容：

![网页搜索演示](assets/OpenWebSearch.png)

### 📹 演示视频
完整的桌面自动化操作演示：

[网页搜索演示](assets/video.mp4)

## 🛠️ 技术栈

- **.NET 10.0**: 基于最新的 .NET 框架
- **Model Context Protocol**: 使用 MCP 协议进行通信
- **Microsoft.Extensions.Hosting**: 应用程序托管框架
- **Serilog**: 结构化日志记录
- **HtmlAgilityPack**: HTML 解析和网页抓取
- **ReverseMarkdown**: HTML 到 Markdown 转换

## 🏗️ 项目结构

```
src/
├── Windows-MCP.Net/         # 主项目
│   ├── .mcp/                # MCP 服务器配置
│   │   └── server.json      # 服务器配置文件
│   ├── Exceptions/          # 自定义异常类（待扩展）
│   ├── Interface/           # 服务接口定义
│   │   ├── IDesktopService.cs   # 桌面服务接口
│   │   ├── IFileSystemService.cs # 文件系统服务接口
│   │   └── IOcrService.cs       # OCR服务接口
│   ├── Models/              # 数据模型（待扩展）
│   ├── Prompts/             # 提示模板（待扩展）
│   ├── Services/            # 核心服务实现
│   │   ├── DesktopService.cs    # 桌面操作服务
│   │   ├── FileSystemService.cs # 文件系统服务
│   │   └── OcrService.cs        # OCR服务
│   ├── Tools/               # MCP 工具实现
│   │   ├── Desktop/             # 桌面操作工具
│   │   │   ├── ClickTool.cs         # 点击工具
│   │   │   ├── ClipboardTool.cs     # 剪贴板工具
│   │   │   ├── DragTool.cs          # 拖拽工具
│   │   │   ├── GetWindowInfoTool.cs # 窗口信息工具
│   │   │   ├── KeyTool.cs           # 按键工具
│   │   │   ├── LaunchTool.cs        # 启动应用工具
│   │   │   ├── MoveTool.cs          # 鼠标移动工具
│   │   │   ├── OpenBrowserTool.cs   # 浏览器打开工具
│   │   │   ├── PowershellTool.cs    # PowerShell执行工具
│   │   │   ├── ResizeTool.cs        # 窗口调整工具
│   │   │   ├── ScrapeTool.cs        # 网页抓取工具
│   │   │   ├── ScreenshotTool.cs    # 截图工具
│   │   │   ├── ScrollTool.cs        # 滚动工具
│   │   │   ├── ShortcutTool.cs      # 快捷键工具
│   │   │   ├── StateTool.cs         # 桌面状态工具
│   │   │   ├── SwitchTool.cs        # 应用切换工具
│   │   │   ├── TypeTool.cs          # 文本输入工具
│   │   │   ├── UIElementTool.cs     # UI元素操作工具
│   │   │   └── WaitTool.cs          # 等待工具
│   │   ├── FileSystem/          # 文件系统工具
│   │   │   ├── CopyFileTool.cs      # 文件复制工具
│   │   │   ├── CreateDirectoryTool.cs # 目录创建工具
│   │   │   ├── CreateFileTool.cs    # 文件创建工具
│   │   │   ├── DeleteDirectoryTool.cs # 目录删除工具
│   │   │   ├── DeleteFileTool.cs    # 文件删除工具
│   │   │   ├── GetFileInfoTool.cs   # 文件信息工具
│   │   │   ├── ListDirectoryTool.cs # 目录列表工具
│   │   │   ├── MoveFileTool.cs      # 文件移动工具
│   │   │   ├── ReadFileTool.cs      # 文件读取工具
│   │   │   ├── SearchFilesTool.cs   # 文件搜索工具
│   │   │   └── WriteFileTool.cs     # 文件写入工具
│   │   └── OCR/                 # OCR识别工具
│   │       ├── ExtractTextFromRegionTool.cs # 区域文本提取工具
│   │       ├── ExtractTextFromScreenTool.cs # 屏幕文本提取工具
│   │       ├── FindTextOnScreenTool.cs      # 屏幕文本查找工具
│   │       └── GetTextCoordinatesTool.cs    # 文本坐标获取工具
│   ├── Program.cs           # 程序入口点
│   └── Windows-MCP.Net.csproj   # 项目文件
└── Windows-MCP.Net.Test/    # 测试项目
    ├── DesktopToolsExtendedTest.cs  # 桌面工具扩展测试
    ├── FileSystemToolsExtendedTest.cs # 文件系统工具扩展测试
    ├── OCRToolsExtendedTest.cs      # OCR工具扩展测试
    ├── ToolTest.cs                  # 工具基础测试
    ├── UIElementToolTest.cs         # UI元素工具测试
    └── Windows-MCP.Net.Test.csproj  # 测试项目文件
```

## 🚧 功能扩展建议

### 计划中的功能

#### 高级UI识别与交互
- **UI元素识别增强**: 支持更多UI框架（WPF、WinForms、UWP）
- **OCR文字识别优化**: 多语言支持，提升识别准确率
- **智能等待机制**: 动态等待元素加载完成

#### 文件系统操作增强
- **高级文件搜索**: 支持内容搜索、正则表达式匹配
- **批量文件操作**: 支持批量复制、移动、重命名
- **文件监控**: 实时监控文件系统变化

#### 系统监控与性能分析
- **系统资源监控**: CPU、内存、磁盘、网络使用情况
- **进程管理**: 进程列表获取、性能监控、进程控制
- **性能分析报告**: 生成详细的系统性能报告

#### 多媒体处理能力
- **音频控制**: 系统音量控制、音频设备管理
- **图像处理**: 图片缩放、裁剪、格式转换
- **屏幕录制**: 支持屏幕录制和回放

#### 网络与通信功能
- **网络诊断**: Ping、端口扫描、连通性测试
- **HTTP客户端**: 支持RESTful API调用
- **WiFi管理**: WiFi网络扫描和连接管理

#### 安全性与权限管理
- **权限检查**: 用户权限验证和管理
- **数据加密**: 敏感数据加密存储
- **操作审计**: 完整的操作日志和审计追踪

### 开发路线图

#### 第一阶段（高优先级）- 核心功能增强
- ✅ UI元素识别工具（已完成Windows API实现）
- 🔄 文件管理工具增强
- 📋 系统监控工具
- 🔒 基础安全工具

#### 第二阶段（中优先级）- 功能扩展
- 📋 OCR文字识别优化
- 📋 高级文件搜索
- 📋 音频控制工具
- 📋 网络诊断工具
- 📋 Excel操作支持

#### 第三阶段（低优先级）- 高级功能
- 📋 图像处理工具
- 📋 任务调度系统
- 📋 数据库操作支持
- 📋 宏录制与回放

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

## 📝 许可证

本项目基于 MIT 许可证开源。详情请参阅 [LICENSE](LICENSE) 文件。

## 🔗 相关链接

- [Model Context Protocol](https://modelcontextprotocol.io/)
- [.NET 文档](https://docs.microsoft.com/dotnet/)
- [Windows API 文档](https://docs.microsoft.com/windows/win32/)

## 🤝 贡献指南

我们欢迎社区贡献！如果您想为项目做出贡献，请遵循以下步骤：

### 开发环境设置

1. **克隆仓库**
   ```bash
   git clone https://github.com/AIDotNet/Windows-MCP.Net.git
   cd Windows-MCP.Net
   ```

2. **安装依赖**
   ```bash
   dotnet restore
   ```

3. **运行测试**
   ```bash
   dotnet test
   ```

4. **构建项目**
   ```bash
   dotnet build
   ```

### 贡献流程

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

### 代码规范

- 遵循 C# 编码规范
- 为新功能添加单元测试
- 更新相关文档
- 确保所有测试通过

### 问题报告

在报告问题时，请提供：
- 操作系统版本
- .NET 版本
- 详细的错误信息
- 重现步骤

## 📞 支持

如果您遇到问题或有建议，请：

1. 查看 [Issues](https://github.com/xuzeyu91/Windows-MCP.Net/issues)
2. 创建新的 Issue
3. 参与讨论
4. 查看 [Wiki](https://github.com/xuzeyu91/Windows-MCP.Net/wiki) 获取更多帮助
---

**注意**: 本工具需要适当的 Windows 权限来执行桌面自动化操作。请确保在受信任的环境中使用。

**免责声明**: 使用本工具进行自动化操作时，请遵守相关法律法规和软件使用协议。开发者不承担因误用工具而产生的任何责任。
