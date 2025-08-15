using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// ScrollTool单元测试类
    /// </summary>
    public class ScrollToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<ScrollTool>> _mockLogger;

        public ScrollToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<ScrollTool>>();
        }

        [Fact]
        public async Task ScrollAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Scroll completed";
            _mockDesktopService.Setup(x => x.ScrollAsync(null, null, "vertical", "down", 1))
                              .ReturnsAsync(expectedResult);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await scrollTool.ScrollAsync();

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrollAsync(null, null, "vertical", "down", 1), Times.Once);
        }

        [Theory]
        [InlineData("vertical", "up", 3)]
        [InlineData("horizontal", "left", 2)]
        [InlineData("vertical", "down", 5)]
        public async Task ScrollAsync_WithParameters_ShouldCallService(string type, string direction, int wheelTimes)
        {
            // Arrange
            var expectedResult = $"Scrolled {direction} {wheelTimes} times";
            _mockDesktopService.Setup(x => x.ScrollAsync(100, 200, type, direction, wheelTimes))
                              .ReturnsAsync(expectedResult);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await scrollTool.ScrollAsync(100, 200, type, direction, wheelTimes);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrollAsync(100, 200, type, direction, wheelTimes), Times.Once);
        }

        [Fact]
        public async Task ScrollAsync_WithDefaultParameters_ShouldUseDefaults()
        {
            // Arrange
            var expectedResult = "Default scroll completed";
            _mockDesktopService.Setup(x => x.ScrollAsync(null, null, "vertical", "down", 1))
                              .ReturnsAsync(expectedResult);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await scrollTool.ScrollAsync();

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrollAsync(null, null, "vertical", "down", 1), Times.Once);
        }

        [Theory]
        [InlineData("up")]
        [InlineData("down")]
        [InlineData("left")]
        [InlineData("right")]
        public async Task ScrollAsync_WithDifferentDirections_ShouldCallService(string direction)
        {
            // Arrange
            var expectedResult = $"Scrolled {direction}";
            _mockDesktopService.Setup(x => x.ScrollAsync(150, 250, "vertical", direction, 1))
                              .ReturnsAsync(expectedResult);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await scrollTool.ScrollAsync(150, 250, "vertical", direction);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrollAsync(150, 250, "vertical", direction, 1), Times.Once);
        }

        [Theory]
        [InlineData("vertical")]
        [InlineData("horizontal")]
        public async Task ScrollAsync_WithDifferentTypes_ShouldCallService(string scrollType)
        {
            // Arrange
            var expectedResult = $"Scrolled {scrollType}";
            _mockDesktopService.Setup(x => x.ScrollAsync(300, 400, scrollType, "down", 1))
                              .ReturnsAsync(expectedResult);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await scrollTool.ScrollAsync(300, 400, scrollType);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrollAsync(300, 400, scrollType, "down", 1), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ScrollAsync_WithDifferentWheelTimes_ShouldCallService(int wheelTimes)
        {
            // Arrange
            var expectedResult = $"Scrolled {wheelTimes} times";
            _mockDesktopService.Setup(x => x.ScrollAsync(200, 300, "vertical", "down", wheelTimes))
                              .ReturnsAsync(expectedResult);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await scrollTool.ScrollAsync(200, 300, "vertical", "down", wheelTimes);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrollAsync(200, 300, "vertical", "down", wheelTimes), Times.Once);
        }

        [Fact]
        public async Task ScrollAsync_WithNullCoordinates_ShouldCallService()
        {
            // Arrange
            var expectedResult = "Scrolled at current position";
            _mockDesktopService.Setup(x => x.ScrollAsync(null, null, "vertical", "up", 2))
                              .ReturnsAsync(expectedResult);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await scrollTool.ScrollAsync(null, null, "vertical", "up", 2);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrollAsync(null, null, "vertical", "up", 2), Times.Once);
        }

        [Fact]
        public async Task ScrollAsync_HorizontalLeftScroll_ShouldCallService()
        {
            // Arrange
            var expectedResult = "Horizontal left scroll completed";
            _mockDesktopService.Setup(x => x.ScrollAsync(500, 600, "horizontal", "left", 3))
                              .ReturnsAsync(expectedResult);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await scrollTool.ScrollAsync(500, 600, "horizontal", "left", 3);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrollAsync(500, 600, "horizontal", "left", 3), Times.Once);
        }

        [Fact]
        public async Task ScrollAsync_HorizontalRightScroll_ShouldCallService()
        {
            // Arrange
            var expectedResult = "Horizontal right scroll completed";
            _mockDesktopService.Setup(x => x.ScrollAsync(600, 700, "horizontal", "right", 2))
                              .ReturnsAsync(expectedResult);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await scrollTool.ScrollAsync(600, 700, "horizontal", "right", 2);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrollAsync(600, 700, "horizontal", "right", 2), Times.Once);
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

            foreach (var (x, y, type, direction, wheelTimes) in scrollOperations)
            {
                _mockDesktopService.Setup(s => s.ScrollAsync(x, y, type, direction, wheelTimes))
                                  .ReturnsAsync($"Scrolled {type} {direction} {wheelTimes} times");
            }

            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            foreach (var (x, y, type, direction, wheelTimes) in scrollOperations)
            {
                var result = await scrollTool.ScrollAsync(x, y, type, direction, wheelTimes);
                Assert.Equal($"Scrolled {type} {direction} {wheelTimes} times", result);
                _mockDesktopService.Verify(s => s.ScrollAsync(x, y, type, direction, wheelTimes), Times.Once);
            }
        }

        [Fact]
        public async Task ScrollAsync_ServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var exception = new InvalidOperationException("Scroll service error");
            _mockDesktopService.Setup(x => x.ScrollAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                              .ThrowsAsync(exception);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => scrollTool.ScrollAsync(100, 200));
            Assert.Equal("Scroll service error", thrownException.Message);
        }
    }
}
