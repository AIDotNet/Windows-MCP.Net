using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// ClickTool单元测试类
    /// </summary>
    public class ClickToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<ClickTool>> _mockLogger;

        public ClickToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<ClickTool>>();
        }

        [Fact]
        public async Task ClickAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Click successful";
            _mockDesktopService.Setup(x => x.ClickAsync(100, 200, "left", 1))
                               .ReturnsAsync(expectedResult);
            var clickTool = new ClickTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clickTool.ClickAsync(100, 200, "left", 1);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClickAsync(100, 200, "left", 1), Times.Once);
        }

        [Theory]
        [InlineData("left", 1)]
        [InlineData("right", 1)]
        [InlineData("middle", 2)]
        [InlineData("left", 3)]
        public async Task ClickAsync_WithDifferentParameters_ShouldCallService(string button, int clicks)
        {
            // Arrange
            var expectedResult = $"Click with {button} button, {clicks} clicks";
            _mockDesktopService.Setup(x => x.ClickAsync(It.IsAny<int>(), It.IsAny<int>(), button, clicks))
                               .ReturnsAsync(expectedResult);
            var clickTool = new ClickTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clickTool.ClickAsync(50, 75, button, clicks);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClickAsync(50, 75, button, clicks), Times.Once);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1920, 1080)]
        [InlineData(500, 300)]
        public async Task ClickAsync_WithDifferentCoordinates_ShouldCallService(int x, int y)
        {
            // Arrange
            var expectedResult = $"Clicked at ({x}, {y})";
            _mockDesktopService.Setup(s => s.ClickAsync(x, y, "left", 1))
                              .ReturnsAsync(expectedResult);
            var clickTool = new ClickTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clickTool.ClickAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(s => s.ClickAsync(x, y, "left", 1), Times.Once);
        }

        [Fact]
        public async Task ClickAsync_WithDefaultParameters_ShouldUseDefaults()
        {
            // Arrange
            var expectedResult = "Default click successful";
            _mockDesktopService.Setup(x => x.ClickAsync(100, 200, "left", 1))
                               .ReturnsAsync(expectedResult);
            var clickTool = new ClickTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clickTool.ClickAsync(100, 200);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClickAsync(100, 200, "left", 1), Times.Once);
        }

        [Fact]
        public async Task ClickAsync_WithDoubleClick_ShouldCallServiceCorrectly()
        {
            // Arrange
            var expectedResult = "Double click completed";
            _mockDesktopService.Setup(x => x.ClickAsync(300, 400, "left", 2))
                               .ReturnsAsync(expectedResult);
            var clickTool = new ClickTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clickTool.ClickAsync(300, 400, "left", 2);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClickAsync(300, 400, "left", 2), Times.Once);
        }

        [Fact]
        public async Task ClickAsync_WithRightClick_ShouldCallServiceCorrectly()
        {
            // Arrange
            var expectedResult = "Right click completed";
            _mockDesktopService.Setup(x => x.ClickAsync(150, 250, "right", 1))
                               .ReturnsAsync(expectedResult);
            var clickTool = new ClickTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clickTool.ClickAsync(150, 250, "right");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClickAsync(150, 250, "right", 1), Times.Once);
        }

        [Fact]
        public async Task ClickAsync_WithTripleClick_ShouldCallServiceCorrectly()
        {
            // Arrange
            var expectedResult = "Triple click completed";
            _mockDesktopService.Setup(x => x.ClickAsync(600, 700, "left", 3))
                               .ReturnsAsync(expectedResult);
            var clickTool = new ClickTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clickTool.ClickAsync(600, 700, "left", 3);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClickAsync(600, 700, "left", 3), Times.Once);
        }
    }
}
