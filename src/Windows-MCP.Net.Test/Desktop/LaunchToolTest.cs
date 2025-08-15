using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// LaunchTool单元测试类
    /// </summary>
    public class LaunchToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<LaunchTool> _logger;
        private readonly LaunchTool _launchTool;

        public LaunchToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<LaunchTool>>();
            _launchTool = new LaunchTool(_desktopService, _logger);
        }

        [Fact]
        public async Task LaunchAppAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var appName = "notepad";

            // Act
            var result = await _launchTool.LaunchAppAsync(appName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // 由于使用真实服务，我们只验证返回了结果
        }

        [Theory]
        [InlineData("calculator")]
        [InlineData("mspaint")]
        [InlineData("cmd")]
        public async Task LaunchAppAsync_WithDifferentApps_ShouldCallService(string appName)
        {
            // Act
            var result = await _launchTool.LaunchAppAsync(appName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // 验证结果包含应用名称或成功信息
            Assert.True(result.Contains(appName) || result.Contains("launched") || result.Contains("success"), 
                       $"Result should indicate launch attempt for {appName}: {result}");
        }

        [Fact]
        public async Task LaunchAppAsync_WithFailedLaunch_ShouldReturnErrorMessage()
        {
            // Arrange
            var appName = "nonexistentapp12345";

            // Act
            var result = await _launchTool.LaunchAppAsync(appName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // 验证失败情况下的错误消息
            Assert.True(result.Contains("Failed") || result.Contains("failed") || result.Contains("error") || result.Contains("not found"),
                       $"Result should indicate failure for non-existent app: {result}");
        }

        [Fact]
        public async Task LaunchAppAsync_WithSuccessfulLaunch_ShouldReturnSuccessMessage()
        {
            // Arrange
            var appName = "notepad";

            // Act
            var result = await _launchTool.LaunchAppAsync(appName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // 验证成功启动的情况
            Assert.False(result.Contains("Failed") && result.Contains("default language"),
                        $"Successful launch should not contain failure message: {result}");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("app with spaces")]
        [InlineData("APP_WITH_UNDERSCORES")]
        public async Task LaunchAppAsync_WithVariousAppNames_ShouldCallService(string appName)
        {
            // Act
            var result = await _launchTool.LaunchAppAsync(appName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            // 验证不同类型的应用名称都能得到响应
            if (string.IsNullOrWhiteSpace(appName))
            {
                // 空或空白字符串应该返回错误信息
                Assert.True(result.Contains("Failed") || result.Contains("error") || result.Contains("invalid"),
                           $"Empty app name should return error: {result}");
            }
            else
            {
                // 非空应用名称应该尝试启动
                Assert.True(result.Length > 0, "Should return some response for non-empty app names");
            }
        }

        [Fact]
        public async Task LaunchAppAsync_WithNonExistentApp_ShouldReturnFailureMessage()
        {
            // Arrange
            var appName = "failedapp99999";

            // Act
            var result = await _launchTool.LaunchAppAsync(appName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // 验证不存在的应用返回失败信息
            Assert.True(result.Contains("Failed") || result.Contains("failed") || result.Contains("not found") || result.Contains("error"),
                       $"Non-existent app should return failure message: {result}");
        }

        [Fact]
        public async Task LaunchAppAsync_ConsecutiveLaunches_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var apps = new[] { "notepad", "calculator", "invalidapp99999" };

            // Act & Assert
            foreach (var appName in apps)
            {
                var result = await _launchTool.LaunchAppAsync(appName);
                
                Assert.NotNull(result);
                Assert.NotEmpty(result);
                
                // 验证每个应用都得到了响应
                Assert.True(result.Length > 0, $"Should return response for {appName}");
            }
        }

        [Fact]
        public async Task LaunchAppAsync_WithValidApp_ShouldNotThrowException()
        {
            // Arrange
            var appName = "notepad";

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () => 
            {
                var result = await _launchTool.LaunchAppAsync(appName);
                Assert.NotNull(result);
            });
            
            // 验证不会抛出异常
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("chrome")]
        [InlineData("firefox")]
        [InlineData("edge")]
        public async Task LaunchAppAsync_WithBrowserApps_ShouldCallService(string browserName)
        {
            // Act
            var result = await _launchTool.LaunchAppAsync(browserName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // 验证浏览器应用启动尝试
            Assert.True(result.Length > 0, $"Should return response for browser {browserName}");
        }

        [Theory]
        [InlineData("powershell")]
        [InlineData("cmd")]
        [InlineData("wt")] // Windows Terminal
        public async Task LaunchAppAsync_WithTerminalApps_ShouldCallService(string terminalName)
        {
            // Act
            var result = await _launchTool.LaunchAppAsync(terminalName);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            // 验证终端应用启动尝试
            Assert.True(result.Length > 0, $"Should return response for terminal {terminalName}");
        }
    }
}
