using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// TypeTool单元测试类
    /// </summary>
    public class TypeToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<TypeTool>> _mockLogger;

        public TypeToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<TypeTool>>();
        }

        [Fact]
        public async Task TypeAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Type successful";
            _mockDesktopService.Setup(x => x.TypeAsync(100, 200, "Hello World", false, false))
                               .ReturnsAsync(expectedResult);
            var typeTool = new TypeTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await typeTool.TypeAsync(100, 200, "Hello World", false, false);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TypeAsync(100, 200, "Hello World", false, false), Times.Once);
        }

        [Theory]
        [InlineData("Test text", true, false)]
        [InlineData("Another text", false, true)]
        [InlineData("Complex text with symbols!@#", true, true)]
        public async Task TypeAsync_WithDifferentParameters_ShouldCallService(string text, bool clear, bool pressEnter)
        {
            // Arrange
            var expectedResult = "Type operation completed";
            _mockDesktopService.Setup(x => x.TypeAsync(It.IsAny<int>(), It.IsAny<int>(), text, clear, pressEnter))
                               .ReturnsAsync(expectedResult);
            var typeTool = new TypeTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await typeTool.TypeAsync(300, 400, text, clear, pressEnter);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TypeAsync(300, 400, text, clear, pressEnter), Times.Once);
        }

        [Fact]
        public async Task TypeAsync_WithDefaultParameters_ShouldUseDefaults()
        {
            // Arrange
            var expectedResult = "Default type successful";
            _mockDesktopService.Setup(x => x.TypeAsync(150, 250, "Default text", false, false))
                               .ReturnsAsync(expectedResult);
            var typeTool = new TypeTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await typeTool.TypeAsync(150, 250, "Default text");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TypeAsync(150, 250, "Default text", false, false), Times.Once);
        }

        [Fact]
        public async Task TypeAsync_WithClearFlag_ShouldCallServiceCorrectly()
        {
            // Arrange
            var expectedResult = "Text cleared and typed";
            _mockDesktopService.Setup(x => x.TypeAsync(200, 300, "New text", true, false))
                               .ReturnsAsync(expectedResult);
            var typeTool = new TypeTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await typeTool.TypeAsync(200, 300, "New text", true);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TypeAsync(200, 300, "New text", true, false), Times.Once);
        }

        [Fact]
        public async Task TypeAsync_WithPressEnterFlag_ShouldCallServiceCorrectly()
        {
            // Arrange
            var expectedResult = "Text typed and Enter pressed";
            _mockDesktopService.Setup(x => x.TypeAsync(400, 500, "Submit text", false, true))
                               .ReturnsAsync(expectedResult);
            var typeTool = new TypeTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await typeTool.TypeAsync(400, 500, "Submit text", false, true);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TypeAsync(400, 500, "Submit text", false, true), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("A")]
        [InlineData("Hello, 世界! 123 @#$%")]
        [InlineData("Multi\nLine\nText")]
        public async Task TypeAsync_WithDifferentTextTypes_ShouldCallService(string text)
        {
            // Arrange
            var expectedResult = $"Typed: {text}";
            _mockDesktopService.Setup(x => x.TypeAsync(100, 100, text, false, false))
                               .ReturnsAsync(expectedResult);
            var typeTool = new TypeTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await typeTool.TypeAsync(100, 100, text);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TypeAsync(100, 100, text, false, false), Times.Once);
        }

        [Fact]
        public async Task TypeAsync_WithBothClearAndEnterFlags_ShouldCallServiceCorrectly()
        {
            // Arrange
            var expectedResult = "Text cleared, typed, and Enter pressed";
            _mockDesktopService.Setup(x => x.TypeAsync(250, 350, "Complete operation", true, true))
                               .ReturnsAsync(expectedResult);
            var typeTool = new TypeTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await typeTool.TypeAsync(250, 350, "Complete operation", true, true);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TypeAsync(250, 350, "Complete operation", true, true), Times.Once);
        }

        [Fact]
        public async Task TypeAsync_WithLongText_ShouldCallService()
        {
            // Arrange
            var longText = new string('A', 1000); // 1000字符的长文本
            var expectedResult = "Long text typed successfully";
            _mockDesktopService.Setup(x => x.TypeAsync(500, 600, longText, false, false))
                               .ReturnsAsync(expectedResult);
            var typeTool = new TypeTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await typeTool.TypeAsync(500, 600, longText);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TypeAsync(500, 600, longText, false, false), Times.Once);
        }

        [Fact]
        public async Task TypeAsync_ServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var exception = new InvalidOperationException("Type service error");
            _mockDesktopService.Setup(x => x.TypeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                              .ThrowsAsync(exception);
            var typeTool = new TypeTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => typeTool.TypeAsync(100, 200, "Test"));
            Assert.Equal("Type service error", thrownException.Message);
        }
    }
}
