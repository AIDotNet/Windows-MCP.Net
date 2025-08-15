using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// ShortcutTool单元测试类
    /// </summary>
    public class ShortcutToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<ShortcutTool>> _mockLogger;

        public ShortcutToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<ShortcutTool>>();
        }

        [Fact]
        public async Task ShortcutAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var keys = new[] { "Ctrl", "C" };
            var expectedResult = "Shortcut executed successfully";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task ShortcutAsync_WithCtrlV_ShouldCallService()
        {
            // Arrange
            var keys = new[] { "Ctrl", "V" };
            var expectedResult = $"Executed shortcut: {string.Join("+", keys)}";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task ShortcutAsync_WithAltTab_ShouldCallService()
        {
            // Arrange
            var keys = new[] { "Alt", "Tab" };
            var expectedResult = $"Executed shortcut: {string.Join("+", keys)}";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task ShortcutAsync_WithCtrlShiftN_ShouldCallService()
        {
            // Arrange
            var keys = new[] { "Ctrl", "Shift", "N" };
            var expectedResult = $"Executed shortcut: {string.Join("+", keys)}";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Theory]
        [InlineData(new[] { "Ctrl", "A" })]
        [InlineData(new[] { "Ctrl", "Z" })]
        [InlineData(new[] { "Ctrl", "Y" })]
        [InlineData(new[] { "Ctrl", "S" })]
        public async Task ShortcutAsync_WithCommonShortcuts_ShouldCallService(string[] keys)
        {
            // Arrange
            var expectedResult = $"Executed: {string.Join("+", keys)}";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Theory]
        [InlineData(new[] { "Alt", "F4" })]
        [InlineData(new[] { "Win", "L" })]
        [InlineData(new[] { "Win", "R" })]
        [InlineData(new[] { "Ctrl", "Shift", "Esc" })]
        public async Task ShortcutAsync_WithSystemShortcuts_ShouldCallService(string[] keys)
        {
            // Arrange
            var expectedResult = $"System shortcut: {string.Join("+", keys)}";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task ShortcutAsync_WithSingleKey_ShouldCallService()
        {
            // Arrange
            var keys = new[] { "F5" };
            var expectedResult = "Single key shortcut executed";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task ShortcutAsync_WithFunctionKeys_ShouldCallService()
        {
            // Arrange
            var keys = new[] { "Ctrl", "F12" };
            var expectedResult = "Function key shortcut executed";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task ShortcutAsync_WithEmptyArray_ShouldCallService()
        {
            // Arrange
            var keys = new string[0];
            var expectedResult = "Empty shortcut array processed";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task ShortcutAsync_WithComplexCombination_ShouldCallService()
        {
            // Arrange
            var keys = new[] { "Ctrl", "Alt", "Shift", "T" };
            var expectedResult = "Complex shortcut executed";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task ShortcutAsync_ConsecutiveShortcuts_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var shortcutSets = new[]
            {
                new[] { "Ctrl", "C" },
                new[] { "Ctrl", "V" },
                new[] { "Alt", "Tab" }
            };

            foreach (var keys in shortcutSets)
            {
                _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                                  .ReturnsAsync($"Executed {string.Join("+", keys)}");
            }

            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            foreach (var keys in shortcutSets)
            {
                var result = await shortcutTool.ShortcutAsync(keys);
                Assert.Equal($"Executed {string.Join("+", keys)}", result);
                _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
            }
        }

        [Fact]
        public async Task ShortcutAsync_ServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var exception = new InvalidOperationException("Shortcut service error");
            _mockDesktopService.Setup(x => x.ShortcutAsync(It.IsAny<string[]>()))
                              .ThrowsAsync(exception);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => shortcutTool.ShortcutAsync(new[] { "Ctrl", "C" }));
            Assert.Equal("Shortcut service error", thrownException.Message);
        }

        [Theory]
        [InlineData(new[] { "ctrl", "c" })]
        [InlineData(new[] { "CTRL", "C" })]
        [InlineData(new[] { "Ctrl", "c" })]
        public async Task ShortcutAsync_WithDifferentCasing_ShouldCallService(string[] keys)
        {
            // Arrange
            var expectedResult = $"Executed {string.Join("+", keys)}";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }
    }
}
