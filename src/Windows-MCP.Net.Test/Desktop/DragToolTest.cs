using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// DragTool单元测试类
    /// </summary>
    public class DragToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<DragTool> _logger;
        private readonly DragTool _dragTool;

        public DragToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<DragTool>>();
            _dragTool = new DragTool(_desktopService, _logger);
        }

        [Fact]
        public async Task DragAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Dragged from (100,200) to (300,400)";

            // Act
            var result = await _dragTool.DragAsync(100, 200, 300, 400);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, 0, 100, 100)]
        [InlineData(500, 300, 800, 600)]
        [InlineData(1000, 500, 200, 300)]
        public async Task DragAsync_WithDifferentCoordinates_ShouldCallService(int fromX, int fromY, int toX, int toY)
        {
            // Arrange
            var expectedResult = $"Dragged from ({fromX},{fromY}) to ({toX},{toY})";

            // Act
            var result = await _dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task DragAsync_WithNegativeCoordinates_ShouldCallService()
        {
            // Arrange
            var fromX = -10;
            var fromY = -5;
            var toX = 50;
            var toY = 100;
            var expectedResult = $"Dragged from ({fromX},{fromY}) to ({toX},{toY})";

            // Act
            var result = await _dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task DragAsync_WithSameSourceAndDestination_ShouldCallService()
        {
            // Arrange
            var x = 200;
            var y = 300;
            var expectedResult = $"Dragged from ({x},{y}) to ({x},{y})";

            // Act
            var result = await _dragTool.DragAsync(x, y, x, y);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task DragAsync_WithLargeCoordinates_ShouldCallService()
        {
            // Arrange
            var fromX = 9999;
            var fromY = 9999;
            var toX = 10000;
            var toY = 10000;
            var expectedResult = $"Dragged from ({fromX},{fromY}) to ({toX},{toY})";

            // Act
            var result = await _dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task DragAsync_HorizontalDrag_ShouldCallService()
        {
            // Arrange
            var fromX = 100;
            var fromY = 200;
            var toX = 500;
            var toY = 200; // 相同的Y坐标，水平拖拽
            var expectedResult = $"Dragged from ({fromX},{fromY}) to ({toX},{toY})";

            // Act
            var result = await _dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task DragAsync_VerticalDrag_ShouldCallService()
        {
            // Arrange
            var fromX = 300;
            var fromY = 100;
            var toX = 300; // 相同的X坐标，垂直拖拽
            var toY = 400;
            var expectedResult = $"Dragged from ({fromX},{fromY}) to ({toX},{toY})";

            // Act
            var result = await _dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task DragAsync_DiagonalDrag_ShouldCallService()
        {
            // Arrange
            var fromX = 150;
            var fromY = 150;
            var toX = 450;
            var toY = 450;
            var expectedResult = $"Dragged from ({fromX},{fromY}) to ({toX},{toY})";

            // Act
            var result = await _dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task DragAsync_ReverseDrag_ShouldCallService()
        {
            // Arrange
            var fromX = 500;
            var fromY = 600;
            var toX = 100;
            var toY = 200;
            var expectedResult = $"Dragged from ({fromX},{fromY}) to ({toX},{toY})";

            // Act
            var result = await _dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task DragAsync_WithValidCoordinates_ShouldNotThrowException()
        {
            // Arrange & Act
            var result = await _dragTool.DragAsync(100, 200, 300, 400);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Dragged from (100,200) to (300,400)", result);
        }

        [Fact]
        public async Task DragAsync_MultipleSequentialDrags_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var dragOperations = new[]
            {
                (fromX: 100, fromY: 100, toX: 200, toY: 200),
                (fromX: 200, fromY: 200, toX: 300, toY: 300),
                (fromX: 300, fromY: 300, toX: 400, toY: 400)
            };

            // Act & Assert
            foreach (var (fromX, fromY, toX, toY) in dragOperations)
            {
                var result = await _dragTool.DragAsync(fromX, fromY, toX, toY);
                Assert.Equal($"Dragged from ({fromX},{fromY}) to ({toX},{toY})", result);
            }
        }
    }
}
