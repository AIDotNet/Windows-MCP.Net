using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// KeyTool单元测试类
    /// </summary>
    public class KeyToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<KeyTool> _logger;
        private readonly KeyTool _keyTool;

        public KeyToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<KeyTool>>();
            _keyTool = new KeyTool(_desktopService, _logger);
        }

        [Fact]
        public async Task KeyAsync_ShouldReturnSuccessMessage()
        {
            // Act
            var result = await _keyTool.KeyAsync("Enter");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Enter", result);
        }

        [Theory]
        [InlineData("Escape")]
        [InlineData("Tab")]
        [InlineData("Space")]
        [InlineData("F1")]
        public async Task KeyAsync_WithDifferentKeys_ShouldCallService(string key)
        {

            // Act
            var result = await _keyTool.KeyAsync(key);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(key, result);
        }

        [Theory]
        [InlineData("Up")]
        [InlineData("Down")]
        [InlineData("Left")]
        [InlineData("Right")]
        public async Task KeyAsync_WithArrowKeys_ShouldCallService(string arrowKey)
        {
            // Act
            var result = await _keyTool.KeyAsync(arrowKey);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(arrowKey, result);
        }

        [Theory]
        [InlineData("F1")]
        [InlineData("F5")]
        [InlineData("F10")]
        [InlineData("F12")]
        public async Task KeyAsync_WithFunctionKeys_ShouldCallService(string functionKey)
        {
            // Act
            var result = await _keyTool.KeyAsync(functionKey);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(functionKey, result);
        }

        [Theory]
        [InlineData("Backspace")]
        [InlineData("Delete")]
        [InlineData("Home")]
        [InlineData("End")]
        [InlineData("PageUp")]
        [InlineData("PageDown")]
        public async Task KeyAsync_WithSpecialKeys_ShouldCallService(string specialKey)
        {
            // Act
            var result = await _keyTool.KeyAsync(specialKey);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(specialKey, result);
        }

        [Fact]
        public async Task KeyAsync_WithEnterKey_ShouldCallService()
        {
            // Act
            var result = await _keyTool.KeyAsync("Enter");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Enter", result);
        }

        [Fact]
        public async Task KeyAsync_WithEscapeKey_ShouldCallService()
        {
            // Act
            var result = await _keyTool.KeyAsync("Escape");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Escape", result);
        }

        [Fact]
        public async Task KeyAsync_WithTabKey_ShouldCallService()
        {
            // Act
            var result = await _keyTool.KeyAsync("Tab");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Tab", result);
        }

        [Fact]
        public async Task KeyAsync_ConsecutiveKeyPresses_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var keys = new[] { "Enter", "Tab", "Escape", "Space" };

            // Act & Assert
            foreach (var key in keys)
            {
                var result = await _keyTool.KeyAsync(key);
                Assert.NotNull(result);
                Assert.Contains(key, result);
            }
        }

        [Fact]
        public async Task KeyAsync_WithInvalidKey_ShouldHandleGracefully()
        {
            // Act
            var result = await _keyTool.KeyAsync("InvalidKey");

            // Assert
            Assert.NotNull(result);
            // 真实服务可能返回错误信息或处理无效按键
        }

        [Theory]
        [InlineData("enter")]
        [InlineData("ENTER")]
        [InlineData("Enter")]
        public async Task KeyAsync_WithDifferentCasing_ShouldCallService(string key)
        {
            // Act
            var result = await _keyTool.KeyAsync(key);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(key, result, StringComparison.OrdinalIgnoreCase);
        }
    }
}
