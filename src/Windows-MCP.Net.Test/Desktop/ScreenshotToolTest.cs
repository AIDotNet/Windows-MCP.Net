using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// ScreenshotTool单元测试类
    /// </summary>
    public class ScreenshotToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<ScreenshotTool>> _mockLogger;

        public ScreenshotToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<ScreenshotTool>>();
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldReturnImagePath()
        {
            // Arrange
            var expectedResult = "C:\\temp\\screenshot.png";
            _mockDesktopService.Setup(x => x.TakeScreenshotAsync())
                              .ReturnsAsync(expectedResult);
            var screenshotTool = new ScreenshotTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TakeScreenshotAsync(), Times.Once);
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldReturnValidFilePath()
        {
            // Arrange
            var expectedPath = "C:\\Windows\\Temp\\screenshot_20240101_120000.png";
            _mockDesktopService.Setup(x => x.TakeScreenshotAsync())
                              .ReturnsAsync(expectedPath);
            var screenshotTool = new ScreenshotTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.Equal(expectedPath, result);
            Assert.Contains("screenshot", result.ToLower());
            Assert.True(result.EndsWith(".png") || result.EndsWith(".jpg") || result.EndsWith(".jpeg"));
            _mockDesktopService.Verify(x => x.TakeScreenshotAsync(), Times.Once);
        }

        [Fact]
        public async Task TakeScreenshotAsync_MultipleConsecutiveCalls_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var screenshots = new[]
            {
                "C:\\temp\\screenshot_1.png",
                "C:\\temp\\screenshot_2.png",
                "C:\\temp\\screenshot_3.png"
            };

            var setupSequence = _mockDesktopService.SetupSequence(x => x.TakeScreenshotAsync());
            foreach (var screenshot in screenshots)
            {
                setupSequence = setupSequence.ReturnsAsync(screenshot);
            }

            var screenshotTool = new ScreenshotTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            for (int i = 0; i < screenshots.Length; i++)
            {
                var result = await screenshotTool.TakeScreenshotAsync();
                Assert.Equal(screenshots[i], result);
            }

            _mockDesktopService.Verify(x => x.TakeScreenshotAsync(), Times.Exactly(3));
        }

        [Fact]
        public async Task TakeScreenshotAsync_ServiceReturnsEmptyString_ShouldReturnEmptyString()
        {
            // Arrange
            _mockDesktopService.Setup(x => x.TakeScreenshotAsync())
                              .ReturnsAsync(string.Empty);
            var screenshotTool = new ScreenshotTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.Equal(string.Empty, result);
            _mockDesktopService.Verify(x => x.TakeScreenshotAsync(), Times.Once);
        }

        [Fact]
        public async Task TakeScreenshotAsync_ServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var exception = new InvalidOperationException("Screenshot service error");
            _mockDesktopService.Setup(x => x.TakeScreenshotAsync())
                              .ThrowsAsync(exception);
            var screenshotTool = new ScreenshotTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => screenshotTool.TakeScreenshotAsync());
            Assert.Equal("Screenshot service error", thrownException.Message);
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldReturnDifferentPaths()
        {
            // Arrange
            var paths = new[]
            {
                "C:\\temp\\screenshot_001.png",
                "C:\\Users\\User\\Desktop\\capture.jpg",
                "D:\\Screenshots\\screen_20240101.png"
            };

            var callCount = 0;
            _mockDesktopService.Setup(x => x.TakeScreenshotAsync())
                              .ReturnsAsync(() => paths[callCount++]);

            var screenshotTool = new ScreenshotTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            foreach (var expectedPath in paths)
            {
                var result = await screenshotTool.TakeScreenshotAsync();
                Assert.Equal(expectedPath, result);
            }

            _mockDesktopService.Verify(x => x.TakeScreenshotAsync(), Times.Exactly(3));
        }

        [Fact]
        public async Task TakeScreenshotAsync_ServiceReturnsNull_ShouldReturnNull()
        {
            // Arrange
            _mockDesktopService.Setup(x => x.TakeScreenshotAsync())
                              .ReturnsAsync((string)null);
            var screenshotTool = new ScreenshotTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.Null(result);
            _mockDesktopService.Verify(x => x.TakeScreenshotAsync(), Times.Once);
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldHandleVariousFileFormats()
        {
            // Arrange
            var filePaths = new[]
            {
                "C:\\temp\\screenshot.png",
                "C:\\temp\\screenshot.jpg",
                "C:\\temp\\screenshot.jpeg",
                "C:\\temp\\screenshot.bmp"
            };

            var callCount = 0;
            _mockDesktopService.Setup(x => x.TakeScreenshotAsync())
                              .ReturnsAsync(() => filePaths[callCount++]);

            var screenshotTool = new ScreenshotTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            foreach (var expectedPath in filePaths)
            {
                var result = await screenshotTool.TakeScreenshotAsync();
                Assert.Equal(expectedPath, result);
                Assert.Contains("screenshot", result);
            }

            _mockDesktopService.Verify(x => x.TakeScreenshotAsync(), Times.Exactly(4));
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldCallServiceExactlyOnce()
        {
            // Arrange
            var expectedResult = "C:\\temp\\single_screenshot.png";
            _mockDesktopService.Setup(x => x.TakeScreenshotAsync())
                              .ReturnsAsync(expectedResult);
            var screenshotTool = new ScreenshotTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TakeScreenshotAsync(), Times.Once);
            _mockDesktopService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task TakeScreenshotAsync_WithLongFilePath_ShouldReturnCorrectPath()
        {
            // Arrange
            var longPath = "C:\\Very\\Long\\Path\\To\\Screenshots\\Directory\\With\\Many\\Subdirectories\\screenshot_with_very_long_filename_20240101_120000.png";
            _mockDesktopService.Setup(x => x.TakeScreenshotAsync())
                              .ReturnsAsync(longPath);
            var screenshotTool = new ScreenshotTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.Equal(longPath, result);
            _mockDesktopService.Verify(x => x.TakeScreenshotAsync(), Times.Once);
        }
    }
}
