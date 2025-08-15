using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// ClipboardTool单元测试类
    /// </summary>
    public class ClipboardToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<ClipboardTool>> _mockLogger;

        public ClipboardToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<ClipboardTool>>();
        }

        [Fact]
        public async Task ClipboardAsync_CopyMode_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Text copied to clipboard";
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync("copy", "Test text"))
                               .ReturnsAsync(expectedResult);
            var clipboardTool = new ClipboardTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clipboardTool.ClipboardAsync("copy", "Test text");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClipboardOperationAsync("copy", "Test text"), Times.Once);
        }

        [Fact]
        public async Task ClipboardAsync_PasteMode_ShouldReturnClipboardContent()
        {
            // Arrange
            var expectedResult = "Clipboard content";
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync("paste", null))
                               .ReturnsAsync(expectedResult);
            var clipboardTool = new ClipboardTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clipboardTool.ClipboardAsync("paste");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClipboardOperationAsync("paste", null), Times.Once);
        }

        [Theory]
        [InlineData("Hello World")]
        [InlineData("")]
        [InlineData("Special chars: @#$%^&*()")]
        [InlineData("Multi\nLine\nText")]
        [InlineData("中文内容测试")]
        public async Task ClipboardAsync_CopyWithDifferentTexts_ShouldCallService(string text)
        {
            // Arrange
            var expectedResult = $"Copied: {text}";
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync("copy", text))
                               .ReturnsAsync(expectedResult);
            var clipboardTool = new ClipboardTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clipboardTool.ClipboardAsync("copy", text);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClipboardOperationAsync("copy", text), Times.Once);
        }

        [Fact]
        public async Task ClipboardAsync_PasteWithoutText_ShouldCallServiceWithNull()
        {
            // Arrange
            var expectedResult = "Current clipboard content";
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync("paste", null))
                               .ReturnsAsync(expectedResult);
            var clipboardTool = new ClipboardTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clipboardTool.ClipboardAsync("paste", null);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClipboardOperationAsync("paste", null), Times.Once);
        }

        [Fact]
        public async Task ClipboardAsync_CopyLongText_ShouldCallService()
        {
            // Arrange
            var longText = new string('A', 5000); // 5000字符的长文本
            var expectedResult = "Long text copied successfully";
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync("copy", longText))
                               .ReturnsAsync(expectedResult);
            var clipboardTool = new ClipboardTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clipboardTool.ClipboardAsync("copy", longText);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClipboardOperationAsync("copy", longText), Times.Once);
        }

        [Theory]
        [InlineData("copy")]
        [InlineData("paste")]
        [InlineData("COPY")]
        [InlineData("PASTE")]
        public async Task ClipboardAsync_WithDifferentModes_ShouldCallService(string mode)
        {
            // Arrange
            var expectedResult = $"Mode {mode} executed";
            var text = mode.ToLower() == "copy" ? "Sample text" : null;
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync(mode, text))
                               .ReturnsAsync(expectedResult);
            var clipboardTool = new ClipboardTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clipboardTool.ClipboardAsync(mode, text);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClipboardOperationAsync(mode, text), Times.Once);
        }

        [Fact]
        public async Task ClipboardAsync_CopyEmptyString_ShouldCallService()
        {
            // Arrange
            var expectedResult = "Empty string copied";
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync("copy", ""))
                               .ReturnsAsync(expectedResult);
            var clipboardTool = new ClipboardTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await clipboardTool.ClipboardAsync("copy", "");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClipboardOperationAsync("copy", ""), Times.Once);
        }

        [Fact]
        public async Task ClipboardAsync_ServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var exception = new InvalidOperationException("Clipboard service error");
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync(It.IsAny<string>(), It.IsAny<string>()))
                              .ThrowsAsync(exception);
            var clipboardTool = new ClipboardTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => clipboardTool.ClipboardAsync("copy", "test"));
            Assert.Equal("Clipboard service error", thrownException.Message);
        }

        [Fact]
        public async Task ClipboardAsync_ConsecutiveOperations_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var copyResult = "Text copied";
            var pasteResult = "Retrieved text";
            
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync("copy", "test"))
                               .ReturnsAsync(copyResult);
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync("paste", null))
                               .ReturnsAsync(pasteResult);
            
            var clipboardTool = new ClipboardTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var copyResultActual = await clipboardTool.ClipboardAsync("copy", "test");
            var pasteResultActual = await clipboardTool.ClipboardAsync("paste");

            // Assert
            Assert.Equal(copyResult, copyResultActual);
            Assert.Equal(pasteResult, pasteResultActual);
            _mockDesktopService.Verify(x => x.ClipboardOperationAsync("copy", "test"), Times.Once);
            _mockDesktopService.Verify(x => x.ClipboardOperationAsync("paste", null), Times.Once);
        }
    }
}
