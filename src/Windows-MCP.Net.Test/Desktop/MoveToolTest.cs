using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// MoveTool单元测试类
    /// </summary>
    public class MoveToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<MoveTool>> _mockLogger;

        public MoveToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<MoveTool>>();
        }

        [Fact]
        public async Task MoveAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Mouse moved successfully";
            _mockDesktopService.Setup(x => x.MoveAsync(100, 200))
                              .ReturnsAsync(expectedResult);
            var moveTool = new MoveTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await moveTool.MoveAsync(100, 200);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.MoveAsync(100, 200), Times.Once);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1920, 1080)]
        [InlineData(500, 300)]
        public async Task MoveAsync_WithDifferentCoordinates_ShouldCallService(int x, int y)
        {
            // Arrange
            var expectedResult = $"Moved to ({x}, {y})";
            _mockDesktopService.Setup(s => s.MoveAsync(x, y))
                              .ReturnsAsync(expectedResult);
            var moveTool = new MoveTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await moveTool.MoveAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(s => s.MoveAsync(x, y), Times.Once);
        }

        [Fact]
        public async Task MoveAsync_WithNegativeCoordinates_ShouldCallService()
        {
            // Arrange
            var x = -10;
            var y = -5;
            var expectedResult = $"Moved to ({x}, {y})";
            _mockDesktopService.Setup(s => s.MoveAsync(x, y))
                              .ReturnsAsync(expectedResult);
            var moveTool = new MoveTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await moveTool.MoveAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(s => s.MoveAsync(x, y), Times.Once);
        }

        [Fact]
        public async Task MoveAsync_WithLargeCoordinates_ShouldCallService()
        {
            // Arrange
            var x = 9999;
            var y = 9999;
            var expectedResult = $"Moved to ({x}, {y})";
            _mockDesktopService.Setup(s => s.MoveAsync(x, y))
                              .ReturnsAsync(expectedResult);
            var moveTool = new MoveTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await moveTool.MoveAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(s => s.MoveAsync(x, y), Times.Once);
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

            foreach (var (x, y) in positions)
            {
                _mockDesktopService.Setup(s => s.MoveAsync(x, y))
                                  .ReturnsAsync($"Moved to ({x}, {y})");
            }

            var moveTool = new MoveTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            foreach (var (x, y) in positions)
            {
                var result = await moveTool.MoveAsync(x, y);
                Assert.Equal($"Moved to ({x}, {y})", result);
                _mockDesktopService.Verify(s => s.MoveAsync(x, y), Times.Once);
            }
        }

        [Fact]
        public async Task MoveAsync_ToSamePosition_ShouldCallService()
        {
            // Arrange
            var x = 150;
            var y = 150;
            var expectedResult = $"Moved to ({x}, {y})";
            _mockDesktopService.Setup(s => s.MoveAsync(x, y))
                              .ReturnsAsync(expectedResult);
            var moveTool = new MoveTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act - 移动到同一位置两次
            var result1 = await moveTool.MoveAsync(x, y);
            var result2 = await moveTool.MoveAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result1);
            Assert.Equal(expectedResult, result2);
            _mockDesktopService.Verify(s => s.MoveAsync(x, y), Times.Exactly(2));
        }

        [Fact]
        public async Task MoveAsync_ServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var exception = new InvalidOperationException("Service error");
            _mockDesktopService.Setup(x => x.MoveAsync(It.IsAny<int>(), It.IsAny<int>()))
                              .ThrowsAsync(exception);
            var moveTool = new MoveTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => moveTool.MoveAsync(100, 200));
            Assert.Equal("Service error", thrownException.Message);
        }
    }
}
