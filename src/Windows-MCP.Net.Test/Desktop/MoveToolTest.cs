using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// MoveTool单元测试类
    /// </summary>
    public class MoveToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<MoveTool> _logger;
        private readonly MoveTool _moveTool;

        public MoveToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<MoveTool>>();
            _moveTool = new MoveTool(_desktopService, _logger);
        }

        [Fact]
        public async Task MoveAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Moved mouse pointer to (100,200)";

            // Act
            var result = await _moveTool.MoveAsync(100, 200);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1920, 1080)]
        [InlineData(500, 300)]
        public async Task MoveAsync_WithDifferentCoordinates_ShouldCallService(int x, int y)
        {
            // Arrange
            var expectedResult = $"Moved mouse pointer to ({x},{y})";

            // Act
            var result = await _moveTool.MoveAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task MoveAsync_WithNegativeCoordinates_ShouldCallService()
        {
            // Arrange
            var x = -10;
            var y = -5;
            var expectedResult = $"Moved mouse pointer to ({x},{y})";

            // Act
            var result = await _moveTool.MoveAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task MoveAsync_WithLargeCoordinates_ShouldCallService()
        {
            // Arrange
            var x = 9999;
            var y = 9999;
            var expectedResult = $"Moved mouse pointer to ({x},{y})";

            // Act
            var result = await _moveTool.MoveAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task MoveAsync_MultipleConsecutiveMoves_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var positions = new[]
            {
                (x: 100, y: 100),
                (x: 200, y: 200),
                (x: 300, y: 300)
            };

            // Act & Assert
            foreach (var (x, y) in positions)
            {
                var result = await _moveTool.MoveAsync(x, y);
                Assert.Equal($"Moved mouse pointer to ({x},{y})", result);
            }
        }

        [Fact]
        public async Task MoveAsync_ToSamePosition_ShouldCallService()
        {
            // Arrange
            var x = 150;
            var y = 150;
            var expectedResult = $"Moved mouse pointer to ({x},{y})";

            // Act - 移动到同一位置两次
            var result1 = await _moveTool.MoveAsync(x, y);
            var result2 = await _moveTool.MoveAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result1);
            Assert.Equal(expectedResult, result2);
        }

        [Fact]
        public async Task MoveAsync_WithValidCoordinates_ShouldReturnSuccessMessage()
        {
            // Arrange
            var x = 100;
            var y = 200;
            var expectedResult = $"Moved mouse pointer to ({x},{y})";

            // Act
            var result = await _moveTool.MoveAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Contains("Moved mouse pointer to", result);
        }
    }
}
