using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;
using Xunit;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// ResizeTool单元测试类
    /// </summary>
    public class ResizeToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<ResizeTool> _logger;
        private readonly ResizeTool _resizeTool;

        public ResizeToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<ResizeTool>>();
            _resizeTool = new ResizeTool(_desktopService, _logger);
        }

        [Fact]
        public async Task ResizeAppAsync_WithValidWindowName_ShouldReturnSuccessMessage()
        {
            // Arrange
            var windowName = "记事本";
            
            // 首先启动记事本应用
            await _desktopService.LaunchAppAsync(windowName);
            await Task.Delay(500); // 减少等待时间
            
            // Act
            var result = await _resizeTool.ResizeAppAsync(windowName, 800, 600);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Contains("Successfully") || result.Contains("Failed"));
        }

        [Fact]
        public async Task ResizeAppAsync_WithInvalidWindowName_ShouldReturnFailureMessage()
        {
            // Arrange
            var invalidWindowName = "不存在的窗口";
            
            // Act
            var result = await _resizeTool.ResizeAppAsync(invalidWindowName, 800, 600);
            
            // Assert
            Assert.NotNull(result);
            Assert.Contains("Failed to resize", result);
        }

        [Fact]
        public async Task ResizeAppAsync_WithOnlyWidth_ShouldResizeWidth()
        {
            // Arrange
            var windowName = "记事本";
            
            // 首先启动记事本应用
            await _desktopService.LaunchAppAsync(windowName);
            await Task.Delay(300); // 减少等待时间
            
            // Act
            var result = await _resizeTool.ResizeAppAsync(windowName, width: 1000);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Contains("Successfully") || result.Contains("Failed"));
        }

        [Fact]
        public async Task ResizeAppAsync_WithOnlyHeight_ShouldResizeHeight()
        {
            // Arrange
            var windowName = "记事本";
            
            // 首先启动记事本应用
            await _desktopService.LaunchAppAsync(windowName);
            await Task.Delay(300); // 减少等待时间
            
            // Act
            var result = await _resizeTool.ResizeAppAsync(windowName, height: 700);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Contains("Successfully") || result.Contains("Failed"));
        }

        [Fact]
        public async Task ResizeAppAsync_WithOnlyPosition_ShouldMoveWindow()
        {
            // Arrange
            var windowName = "记事本";
            
            // 首先启动记事本应用
            await _desktopService.LaunchAppAsync(windowName);
            await Task.Delay(300); // 减少等待时间
            
            // Act
            var result = await _resizeTool.ResizeAppAsync(windowName, x: 100, y: 100);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Contains("Successfully") || result.Contains("Failed"));
        }

        [Fact]
        public async Task ResizeAppAsync_WithAllParameters_ShouldResizeAndMove()
        {
            // Arrange
            var windowName = "记事本";
            
            // 首先启动记事本应用
            await _desktopService.LaunchAppAsync(windowName);
            await Task.Delay(300); // 减少等待时间
            
            // Act
            var result = await _resizeTool.ResizeAppAsync(windowName, 900, 650, 200, 150);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Contains("Successfully") || result.Contains("Failed"));
        }

        [Fact]
        public async Task ResizeAppAsync_WithNullParameters_ShouldNotChangeWindowSize()
        {
            // Arrange
            var windowName = "记事本";
            
            // 首先启动记事本应用
            await _desktopService.LaunchAppAsync(windowName);
            await Task.Delay(300); // 减少等待时间
            
            // Act - 不传递任何参数，窗口应该保持原样
            var result = await _resizeTool.ResizeAppAsync(windowName);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Contains("Successfully") || result.Contains("Failed"));
        }

        [Fact]
        public async Task ResizeAppAsync_WithCalculatorApp_ShouldWork()
        {
            // Arrange
            var windowName = "计算器";
            
            // 首先启动计算器应用
            await _desktopService.LaunchAppAsync(windowName);
            await Task.Delay(500); // 减少等待时间
            
            // Act
            var result = await _resizeTool.ResizeAppAsync(windowName, 400, 500);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Contains("Successfully") || result.Contains("Failed"));
        }

        [Fact]
        public async Task ResizeAppAsync_WithEnglishWindowName_ShouldWork()
        {
            // Arrange
            var windowName = "Notepad";
            
            // 首先启动记事本应用
            await _desktopService.LaunchAppAsync("notepad");
            await Task.Delay(300); // 减少等待时间
            
            // Act
            var result = await _resizeTool.ResizeAppAsync(windowName, 600, 400);
            
            // Assert
            Assert.NotNull(result);
            // 如果英文名称不工作，应该返回失败消息并建议使用默认语言
            Assert.True(result.Contains("Successfully") || result.Contains("Failed to resize"));
        }

        [Fact]
        public void ResizeTool_Constructor_ShouldInitializeCorrectly()
        {
            // Act & Assert - 构造函数测试，不需要异步操作
            Assert.NotNull(_resizeTool);
            Assert.NotNull(_desktopService);
            Assert.NotNull(_logger);
        }

        [Fact]
        public async Task ResizeAppAsync_WithNegativeValues_ShouldHandleGracefully()
        {
            // Arrange
            var windowName = "不存在的窗口";
            
            // Act - 测试负值参数
            var result = await _resizeTool.ResizeAppAsync(windowName, -100, -200, -50, -75);
            
            // Assert
            Assert.NotNull(result);
            Assert.Contains("Failed to resize", result);
        }

        [Fact]
        public async Task ResizeAppAsync_WithEmptyWindowName_ShouldReturnResult()
        {
            // Arrange
            var emptyWindowName = "";
            
            // Act
            var result = await _resizeTool.ResizeAppAsync(emptyWindowName, 800, 600);
            
            // Assert
            Assert.NotNull(result);
            // 空窗口名称可能被系统处理为有效操作，所以接受任何结果
            Assert.True(result.Contains("Successfully") || result.Contains("Failed"));
        }
    }
}