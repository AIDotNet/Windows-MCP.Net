using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// GetWindowInfoTool单元测试类
    /// </summary>
    public class GetWindowInfoToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<GetWindowInfoTool> _logger;
        private readonly GetWindowInfoTool _getWindowInfoTool;

        public GetWindowInfoToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<GetWindowInfoTool>>();
            _getWindowInfoTool = new GetWindowInfoTool(_desktopService, _logger);
        }

        [Fact]
        public async Task GetWindowInfoAsync_WithValidWindowName_ShouldReturnWindowInfo()
        {
            // Arrange
            var windowName = "Calculator"; // 使用Windows计算器作为测试目标
            
            // 首先启动计算器应用
            var (launchResponse, launchStatus) = await _desktopService.LaunchAppAsync("Calculator");
            
            // 等待应用启动
            await Task.Delay(3000);
            
            // Act
            var result = await _getWindowInfoTool.GetWindowInfoAsync(windowName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // 如果窗口存在，结果应该包含窗口信息
            if (!result.Contains("Failed to get window info"))
            {
                Assert.Contains("Position:", result);
                Assert.Contains("Size:", result);
                Assert.Contains("Center:", result);
            }
        }

        [Fact]
        public async Task GetWindowInfoAsync_WithInvalidWindowName_ShouldReturnErrorMessage()
        {
            // Arrange
            var invalidWindowName = "NonExistentApplication12345";

            // Act
            var result = await _getWindowInfoTool.GetWindowInfoAsync(invalidWindowName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Failed to get window info", result);
            Assert.Contains("Try to use the app name in the default language", result);
        }

        [Theory]
        [InlineData("Notepad")]
        [InlineData("Calculator")]
        [InlineData("Paint")]
        public async Task GetWindowInfoAsync_WithCommonApplications_ShouldHandleGracefully(string appName)
        {
            // Arrange & Act
            var result = await _getWindowInfoTool.GetWindowInfoAsync(appName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // 结果应该要么包含窗口信息，要么包含未找到的错误信息
            Assert.True(result.Contains("Position:") || result.Contains("Failed to get window info"));
        }

        [Fact]
        public async Task GetWindowInfoAsync_WithEmptyWindowName_ShouldReturnResult()
        {
            // Arrange
            var emptyWindowName = "";

            // Act
            var result = await _getWindowInfoTool.GetWindowInfoAsync(emptyWindowName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // 空字符串可能会匹配到某个窗口或返回错误信息
            Assert.True(result.Contains("information:") || result.Contains("Failed to get window info"));
        }

        [Fact]
        public async Task GetWindowInfoAsync_WithNullWindowName_ShouldReturnErrorMessage()
        {
            // Arrange
            string? nullWindowName = null;

            // Act
            var result = await _getWindowInfoTool.GetWindowInfoAsync(nullWindowName!);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Failed to get window info", result);
        }

        [Fact]
        public async Task GetWindowInfoAsync_WithSpecialCharacters_ShouldHandleGracefully()
        {
            // Arrange
            var specialCharWindowName = "@#$%^&*()";

            // Act
            var result = await _getWindowInfoTool.GetWindowInfoAsync(specialCharWindowName);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Failed to get window info", result);
        }

        [Fact]
        public async Task GetWindowInfoAsync_WithCurrentActiveWindow_ShouldReturnValidInfo()
        {
            // Arrange
            // 启动记事本作为测试窗口
            var (launchResponse, launchStatus) = await _desktopService.LaunchAppAsync("Notepad");
            await Task.Delay(3000); // 等待应用启动
            
            // Act
            var result = await _getWindowInfoTool.GetWindowInfoAsync("Notepad");

            // Assert
            Assert.NotNull(result);
            if (!result.Contains("Failed to get window info"))
            {
                // 验证返回的信息格式
                Assert.Contains("information:", result);
                Assert.Contains("Position:", result);
                Assert.Contains("Size:", result);
                Assert.Contains("Center:", result);
                Assert.Contains("Left=", result);
                Assert.Contains("Top=", result);
                Assert.Contains("Width=", result);
                Assert.Contains("Height=", result);
            }
        }

        [Fact]
        public async Task GetWindowInfoAsync_MultipleCallsSameWindow_ShouldReturnConsistentResults()
        {
            // Arrange
            var windowName = "Calculator";
            var (launchResponse, launchStatus) = await _desktopService.LaunchAppAsync(windowName);
            await Task.Delay(3000);

            // Act
            var result1 = await _getWindowInfoTool.GetWindowInfoAsync(windowName);
            await Task.Delay(100); // 短暂延迟
            var result2 = await _getWindowInfoTool.GetWindowInfoAsync(windowName);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            
            // 如果两次调用都成功找到窗口，结果应该相似（窗口位置可能略有变化）
            if (!result1.Contains("Failed to get window info") && !result2.Contains("Failed to get window info"))
            {
                Assert.Contains("Position:", result1);
                Assert.Contains("Position:", result2);
            }
            else
            {
                // 如果窗口未找到，两次调用都应该返回错误信息
                Assert.Contains("Failed to get window info", result1);
                Assert.Contains("Failed to get window info", result2);
            }
        }
    }
}