using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// DragTool单元测试类
    /// </summary>
    public class DragToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<DragTool>> _mockLogger;

        public DragToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<DragTool>>();
        }

        [Fact]
        public async Task DragAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Drag operation completed";
            _mockDesktopService.Setup(x => x.DragAsync(100, 200, 300, 400))
                              .ReturnsAsync(expectedResult);
            var dragTool = new DragTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await dragTool.DragAsync(100, 200, 300, 400);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.DragAsync(100, 200, 300, 400), Times.Once);
        }

        [Theory]
        [InlineData(0, 0, 100, 100)]
        [InlineData(500, 300, 800, 600)]
        [InlineData(1000, 500, 200, 300)]
        public async Task DragAsync_WithDifferentCoordinates_ShouldCallService(int fromX, int fromY, int toX, int toY)
        {
            // Arrange
            var expectedResult = $"Dragged from ({fromX},{fromY}) to ({toX},{toY})";
            _mockDesktopService.Setup(x => x.DragAsync(fromX, fromY, toX, toY))
                              .ReturnsAsync(expectedResult);
            var dragTool = new DragTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.DragAsync(fromX, fromY, toX, toY), Times.Once);
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
            _mockDesktopService.Setup(x => x.DragAsync(fromX, fromY, toX, toY))
                              .ReturnsAsync(expectedResult);
            var dragTool = new DragTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.DragAsync(fromX, fromY, toX, toY), Times.Once);
        }

        [Fact]
        public async Task DragAsync_WithSameSourceAndDestination_ShouldCallService()
        {
            // Arrange
            var x = 200;
            var y = 300;
            var expectedResult = $"Dragged from ({x},{y}) to ({x},{y})";
            _mockDesktopService.Setup(s => s.DragAsync(x, y, x, y))
                              .ReturnsAsync(expectedResult);
            var dragTool = new DragTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await dragTool.DragAsync(x, y, x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(s => s.DragAsync(x, y, x, y), Times.Once);
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
            _mockDesktopService.Setup(x => x.DragAsync(fromX, fromY, toX, toY))
                              .ReturnsAsync(expectedResult);
            var dragTool = new DragTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.DragAsync(fromX, fromY, toX, toY), Times.Once);
        }

        [Fact]
        public async Task DragAsync_HorizontalDrag_ShouldCallService()
        {
            // Arrange
            var fromX = 100;
            var fromY = 200;
            var toX = 500;
            var toY = 200; // 相同的Y坐标，水平拖拽
            var expectedResult = "Horizontal drag completed";
            _mockDesktopService.Setup(x => x.DragAsync(fromX, fromY, toX, toY))
                              .ReturnsAsync(expectedResult);
            var dragTool = new DragTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.DragAsync(fromX, fromY, toX, toY), Times.Once);
        }

        [Fact]
        public async Task DragAsync_VerticalDrag_ShouldCallService()
        {
            // Arrange
            var fromX = 300;
            var fromY = 100;
            var toX = 300; // 相同的X坐标，垂直拖拽
            var toY = 400;
            var expectedResult = "Vertical drag completed";
            _mockDesktopService.Setup(x => x.DragAsync(fromX, fromY, toX, toY))
                              .ReturnsAsync(expectedResult);
            var dragTool = new DragTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.DragAsync(fromX, fromY, toX, toY), Times.Once);
        }

        [Fact]
        public async Task DragAsync_DiagonalDrag_ShouldCallService()
        {
            // Arrange
            var fromX = 150;
            var fromY = 150;
            var toX = 450;
            var toY = 450;
            var expectedResult = "Diagonal drag completed";
            _mockDesktopService.Setup(x => x.DragAsync(fromX, fromY, toX, toY))
                              .ReturnsAsync(expectedResult);
            var dragTool = new DragTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.DragAsync(fromX, fromY, toX, toY), Times.Once);
        }

        [Fact]
        public async Task DragAsync_ReverseDrag_ShouldCallService()
        {
            // Arrange
            var fromX = 500;
            var fromY = 600;
            var toX = 100;
            var toY = 200;
            var expectedResult = "Reverse drag completed";
            _mockDesktopService.Setup(x => x.DragAsync(fromX, fromY, toX, toY))
                              .ReturnsAsync(expectedResult);
            var dragTool = new DragTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await dragTool.DragAsync(fromX, fromY, toX, toY);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.DragAsync(fromX, fromY, toX, toY), Times.Once);
        }

        [Fact]
        public async Task DragAsync_ServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var exception = new InvalidOperationException("Drag service error");
            _mockDesktopService.Setup(x => x.DragAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                              .ThrowsAsync(exception);
            var dragTool = new DragTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => dragTool.DragAsync(100, 200, 300, 400));
            Assert.Equal("Drag service error", thrownException.Message);
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

            foreach (var (fromX, fromY, toX, toY) in dragOperations)
            {
                _mockDesktopService.Setup(x => x.DragAsync(fromX, fromY, toX, toY))
                                  .ReturnsAsync($"Dragged from ({fromX},{fromY}) to ({toX},{toY})");
            }

            var dragTool = new DragTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            foreach (var (fromX, fromY, toX, toY) in dragOperations)
            {
                var result = await dragTool.DragAsync(fromX, fromY, toX, toY);
                Assert.Equal($"Dragged from ({fromX},{fromY}) to ({toX},{toY})", result);
                _mockDesktopService.Verify(x => x.DragAsync(fromX, fromY, toX, toY), Times.Once);
            }
        }
    }
}
