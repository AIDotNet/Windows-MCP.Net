using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// KeyTool单元测试类
    /// </summary>
    public class KeyToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<KeyTool>> _mockLogger;

        public KeyToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<KeyTool>>();
        }

        [Fact]
        public async Task KeyAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Key pressed successfully";
            _mockDesktopService.Setup(x => x.KeyAsync("Enter"))
                              .ReturnsAsync(expectedResult);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await keyTool.KeyAsync("Enter");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.KeyAsync("Enter"), Times.Once);
        }

        [Theory]
        [InlineData("Escape")]
        [InlineData("Tab")]
        [InlineData("Space")]
        [InlineData("F1")]
        public async Task KeyAsync_WithDifferentKeys_ShouldCallService(string key)
        {
            // Arrange
            var expectedResult = $"Pressed {key}";
            _mockDesktopService.Setup(x => x.KeyAsync(key))
                              .ReturnsAsync(expectedResult);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await keyTool.KeyAsync(key);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.KeyAsync(key), Times.Once);
        }

        [Theory]
        [InlineData("Up")]
        [InlineData("Down")]
        [InlineData("Left")]
        [InlineData("Right")]
        public async Task KeyAsync_WithArrowKeys_ShouldCallService(string arrowKey)
        {
            // Arrange
            var expectedResult = $"Arrow key {arrowKey} pressed";
            _mockDesktopService.Setup(x => x.KeyAsync(arrowKey))
                              .ReturnsAsync(expectedResult);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await keyTool.KeyAsync(arrowKey);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.KeyAsync(arrowKey), Times.Once);
        }

        [Theory]
        [InlineData("F1")]
        [InlineData("F5")]
        [InlineData("F10")]
        [InlineData("F12")]
        public async Task KeyAsync_WithFunctionKeys_ShouldCallService(string functionKey)
        {
            // Arrange
            var expectedResult = $"Function key {functionKey} pressed";
            _mockDesktopService.Setup(x => x.KeyAsync(functionKey))
                              .ReturnsAsync(expectedResult);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await keyTool.KeyAsync(functionKey);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.KeyAsync(functionKey), Times.Once);
        }

        [Theory]
        [InlineData("Backspace")]
        [InlineData("Delete")]
        [InlineData("Home")]
        [InlineData("End")]
        [InlineData("PageUp")]
        [InlineData("PageDown")]
        public async Task KeyAsync_WithSpecialKeys_ShouldCallService(string specialKey)
        {
            // Arrange
            var expectedResult = $"Special key {specialKey} pressed";
            _mockDesktopService.Setup(x => x.KeyAsync(specialKey))
                              .ReturnsAsync(expectedResult);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await keyTool.KeyAsync(specialKey);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.KeyAsync(specialKey), Times.Once);
        }

        [Fact]
        public async Task KeyAsync_WithEnterKey_ShouldCallService()
        {
            // Arrange
            var expectedResult = "Enter key pressed";
            _mockDesktopService.Setup(x => x.KeyAsync("Enter"))
                              .ReturnsAsync(expectedResult);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await keyTool.KeyAsync("Enter");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.KeyAsync("Enter"), Times.Once);
        }

        [Fact]
        public async Task KeyAsync_WithEscapeKey_ShouldCallService()
        {
            // Arrange
            var expectedResult = "Escape key pressed";
            _mockDesktopService.Setup(x => x.KeyAsync("Escape"))
                              .ReturnsAsync(expectedResult);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await keyTool.KeyAsync("Escape");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.KeyAsync("Escape"), Times.Once);
        }

        [Fact]
        public async Task KeyAsync_WithTabKey_ShouldCallService()
        {
            // Arrange
            var expectedResult = "Tab key pressed";
            _mockDesktopService.Setup(x => x.KeyAsync("Tab"))
                              .ReturnsAsync(expectedResult);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await keyTool.KeyAsync("Tab");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.KeyAsync("Tab"), Times.Once);
        }

        [Fact]
        public async Task KeyAsync_ConsecutiveKeyPresses_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var keys = new[] { "Enter", "Tab", "Escape", "Space" };
            foreach (var key in keys)
            {
                _mockDesktopService.Setup(x => x.KeyAsync(key))
                                  .ReturnsAsync($"Pressed {key}");
            }
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            foreach (var key in keys)
            {
                var result = await keyTool.KeyAsync(key);
                Assert.Equal($"Pressed {key}", result);
                _mockDesktopService.Verify(x => x.KeyAsync(key), Times.Once);
            }
        }

        [Fact]
        public async Task KeyAsync_ServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var exception = new InvalidOperationException("Key service error");
            _mockDesktopService.Setup(x => x.KeyAsync(It.IsAny<string>()))
                              .ThrowsAsync(exception);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => keyTool.KeyAsync("Enter"));
            Assert.Equal("Key service error", thrownException.Message);
        }

        [Theory]
        [InlineData("enter")]
        [InlineData("ENTER")]
        [InlineData("Enter")]
        public async Task KeyAsync_WithDifferentCasing_ShouldCallService(string key)
        {
            // Arrange
            var expectedResult = $"Key {key} pressed";
            _mockDesktopService.Setup(x => x.KeyAsync(key))
                              .ReturnsAsync(expectedResult);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await keyTool.KeyAsync(key);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.KeyAsync(key), Times.Once);
        }
    }
}
