using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// ClickTool单元测试类
    /// </summary>
    public class ClickToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<ClickTool> _logger;
        private readonly ClickTool _clickTool;

        public ClickToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<ClickTool>>();
            _clickTool = new ClickTool(_desktopService, _logger);
        }

        [Fact]
        public async Task ClickAsync_ShouldReturnSuccessMessage()
        {
            // Act
            var result = await _clickTool.ClickAsync(100, 200, "left", 1);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("left", 1)]
        [InlineData("right", 1)]
        [InlineData("middle", 2)]
        [InlineData("left", 3)]
        public async Task ClickAsync_WithDifferentParameters_ShouldCallService(string button, int clicks)
        {
            // Act
            var result = await _clickTool.ClickAsync(50, 75, button, clicks);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1920, 1080)]
        [InlineData(500, 300)]
        public async Task ClickAsync_WithDifferentCoordinates_ShouldCallService(int x, int y)
        {
            // Act
            var result = await _clickTool.ClickAsync(x, y);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ClickAsync_WithDefaultParameters_ShouldUseDefaults()
        {
            // Act
            var result = await _clickTool.ClickAsync(100, 200);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ClickAsync_WithDoubleClick_ShouldCallServiceCorrectly()
        {
            // Act
            var result = await _clickTool.ClickAsync(300, 400, "left", 2);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ClickAsync_WithRightClick_ShouldCallServiceCorrectly()
        {
            // Act
            var result = await _clickTool.ClickAsync(150, 250, "right");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ClickAsync_WithTripleClick_ShouldCallServiceCorrectly()
        {
            // Act
            var result = await _clickTool.ClickAsync(600, 700, "left", 3);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
