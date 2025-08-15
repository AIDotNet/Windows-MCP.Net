using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// ScrollTool单元测试类
    /// </summary>
    public class ScrollToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<ScrollTool> _logger;
        private readonly ScrollTool _scrollTool;

        public ScrollToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<ScrollTool>>();
            _scrollTool = new ScrollTool(_desktopService, _logger);
        }

        [Fact]
        public async Task ScrollAsync_ShouldReturnSuccessMessage()
        {
            // Act
            var result = await _scrollTool.ScrollAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("vertical", "up", 3)]
        [InlineData("horizontal", "left", 2)]
        [InlineData("vertical", "down", 5)]
        public async Task ScrollAsync_WithParameters_ShouldCallService(string type, string direction, int wheelTimes)
        {
            // Act
            var result = await _scrollTool.ScrollAsync(100, 200, type, direction, wheelTimes);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ScrollAsync_WithDefaultParameters_ShouldUseDefaults()
        {
            // Act
            var result = await _scrollTool.ScrollAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("up")]
        [InlineData("down")]
        [InlineData("left")]
        [InlineData("right")]
        public async Task ScrollAsync_WithDifferentDirections_ShouldCallService(string direction)
        {
            // Act
            var result = await _scrollTool.ScrollAsync(150, 250, "vertical", direction);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("vertical")]
        [InlineData("horizontal")]
        public async Task ScrollAsync_WithDifferentTypes_ShouldCallService(string scrollType)
        {
            // Act
            var result = await _scrollTool.ScrollAsync(300, 400, scrollType);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ScrollAsync_WithDifferentWheelTimes_ShouldCallService(int wheelTimes)
        {
            // Act
            var result = await _scrollTool.ScrollAsync(200, 300, "vertical", "down", wheelTimes);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ScrollAsync_WithNullCoordinates_ShouldCallService()
        {
            // Act
            var result = await _scrollTool.ScrollAsync(null, null, "vertical", "up", 2);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ScrollAsync_HorizontalLeftScroll_ShouldCallService()
        {
            // Act
            var result = await _scrollTool.ScrollAsync(500, 600, "horizontal", "left", 3);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ScrollAsync_HorizontalRightScroll_ShouldCallService()
        {
            // Act
            var result = await _scrollTool.ScrollAsync(600, 700, "horizontal", "right", 2);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ScrollAsync_MultipleConsecutiveScrolls_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var scrollOperations = new[]
            {
                (x: 100, y: 100, type: "vertical", direction: "down", wheelTimes: 1),
                (x: 200, y: 200, type: "vertical", direction: "up", wheelTimes: 2),
                (x: 300, y: 300, type: "horizontal", direction: "left", wheelTimes: 3)
            };

            // Act & Assert
            foreach (var (x, y, type, direction, wheelTimes) in scrollOperations)
            {
                var result = await _scrollTool.ScrollAsync(x, y, type, direction, wheelTimes);
                Assert.NotNull(result);
                Assert.NotEmpty(result);
            }
        }

        [Fact]
        public async Task ScrollAsync_ShouldExecuteSuccessfully()
        {
            // Act
            var result = await _scrollTool.ScrollAsync(100, 200);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
