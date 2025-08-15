using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// WaitTool单元测试类
    /// </summary>
    public class WaitToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<WaitTool>> _mockLogger;

        public WaitToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<WaitTool>>();
        }

        [Fact]
        public async Task WaitAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Wait completed";
            _mockDesktopService.Setup(x => x.WaitAsync(5))
                              .ReturnsAsync(expectedResult);
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await waitTool.WaitAsync(5);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.WaitAsync(5), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(30)]
        public async Task WaitAsync_WithDifferentDurations_ShouldCallService(int duration)
        {
            // Arrange
            var expectedResult = $"Waited for {duration} seconds";
            _mockDesktopService.Setup(x => x.WaitAsync(duration))
                              .ReturnsAsync(expectedResult);
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.WaitAsync(duration), Times.Once);
        }

        [Fact]
        public async Task WaitAsync_WithZeroDuration_ShouldCallService()
        {
            // Arrange
            var duration = 0;
            var expectedResult = "No wait required";
            _mockDesktopService.Setup(x => x.WaitAsync(duration))
                              .ReturnsAsync(expectedResult);
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.WaitAsync(duration), Times.Once);
        }

        [Fact]
        public async Task WaitAsync_WithLargeDuration_ShouldCallService()
        {
            // Arrange
            var duration = 3600; // 1 hour
            var expectedResult = $"Long wait of {duration} seconds completed";
            _mockDesktopService.Setup(x => x.WaitAsync(duration))
                              .ReturnsAsync(expectedResult);
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.WaitAsync(duration), Times.Once);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(15)]
        public async Task WaitAsync_WithCommonDurations_ShouldCallService(int duration)
        {
            // Arrange
            var expectedResult = $"Standard wait of {duration} seconds completed";
            _mockDesktopService.Setup(x => x.WaitAsync(duration))
                              .ReturnsAsync(expectedResult);
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.WaitAsync(duration), Times.Once);
        }

        [Fact]
        public async Task WaitAsync_MultipleConsecutiveWaits_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var durations = new[] { 1, 2, 3 };
            foreach (var duration in durations)
            {
                _mockDesktopService.Setup(x => x.WaitAsync(duration))
                                  .ReturnsAsync($"Waited {duration} seconds");
            }
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            foreach (var duration in durations)
            {
                var result = await waitTool.WaitAsync(duration);
                Assert.Equal($"Waited {duration} seconds", result);
                _mockDesktopService.Verify(x => x.WaitAsync(duration), Times.Once);
            }
        }

        [Fact]
        public async Task WaitAsync_ServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var exception = new InvalidOperationException("Wait service error");
            _mockDesktopService.Setup(x => x.WaitAsync(It.IsAny<int>()))
                              .ThrowsAsync(exception);
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => waitTool.WaitAsync(5));
            Assert.Equal("Wait service error", thrownException.Message);
        }

        [Fact]
        public async Task WaitAsync_WithNegativeDuration_ShouldCallService()
        {
            // Arrange
            var duration = -5;
            var expectedResult = "Invalid duration handled";
            _mockDesktopService.Setup(x => x.WaitAsync(duration))
                              .ReturnsAsync(expectedResult);
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.WaitAsync(duration), Times.Once);
        }

        [Fact]
        public async Task WaitAsync_WithVerySmallDuration_ShouldCallService()
        {
            // Arrange
            var duration = 1;
            var expectedResult = "Quick wait completed";
            _mockDesktopService.Setup(x => x.WaitAsync(duration))
                              .ReturnsAsync(expectedResult);
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.WaitAsync(duration), Times.Once);
        }

        [Fact]
        public async Task WaitAsync_ShouldPassCorrectDurationToService()
        {
            // Arrange
            var duration = 42;
            var expectedResult = "Wait completed successfully";
            _mockDesktopService.Setup(x => x.WaitAsync(duration))
                              .ReturnsAsync(expectedResult);
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.WaitAsync(duration), Times.Once);
            // 验证传递的参数值正确
            _mockDesktopService.Verify(x => x.WaitAsync(It.Is<int>(d => d == 42)), Times.Once);
        }
    }
}
