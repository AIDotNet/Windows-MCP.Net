using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Tools.Desktop;
using Tools.FileSystem;
using WindowsMCP.Net.Services;
using WindowsMCP.Net.Tools.OCR;

namespace Windows_MCP.Net.Test
{
    /// <summary>
    /// Desktop工具测试类
    /// </summary>
    public class DesktopToolsTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<ClickTool>> _mockClickLogger;
        private readonly Mock<ILogger<TypeTool>> _mockTypeLogger;
        private readonly Mock<ILogger<ClipboardTool>> _mockClipboardLogger;

        public DesktopToolsTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockClickLogger = new Mock<ILogger<ClickTool>>();
            _mockTypeLogger = new Mock<ILogger<TypeTool>>();
            _mockClipboardLogger = new Mock<ILogger<ClipboardTool>>();
        }

        [Fact]
        public async Task ClickTool_ClickAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Click successful";
            _mockDesktopService.Setup(x => x.ClickAsync(100, 200, "left", 1))
                               .ReturnsAsync(expectedResult);
            var clickTool = new ClickTool(_mockDesktopService.Object, _mockClickLogger.Object);

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
        public async Task ClickTool_ClickAsync_WithDifferentParameters_ShouldCallService(string button, int clicks)
        {
            // Arrange
            var expectedResult = $"Click with {button} button, {clicks} clicks";
            _mockDesktopService.Setup(x => x.ClickAsync(It.IsAny<int>(), It.IsAny<int>(), button, clicks))
                               .ReturnsAsync(expectedResult);
            var clickTool = new ClickTool(_mockDesktopService.Object, _mockClickLogger.Object);

            // Act
            var result = await clickTool.ClickAsync(50, 75, button, clicks);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClickAsync(50, 75, button, clicks), Times.Once);
        }

        [Fact]
        public async Task TypeTool_TypeAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Type successful";
            _mockDesktopService.Setup(x => x.TypeAsync(100, 200, "Hello World", false, false))
                               .ReturnsAsync(expectedResult);
            var typeTool = new TypeTool(_mockDesktopService.Object, _mockTypeLogger.Object);

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
        public async Task TypeTool_TypeAsync_WithDifferentParameters_ShouldCallService(string text, bool clear, bool pressEnter)
        {
            // Arrange
            var expectedResult = "Type operation completed";
            _mockDesktopService.Setup(x => x.TypeAsync(It.IsAny<int>(), It.IsAny<int>(), text, clear, pressEnter))
                               .ReturnsAsync(expectedResult);
            var typeTool = new TypeTool(_mockDesktopService.Object, _mockTypeLogger.Object);

            // Act
            var result = await typeTool.TypeAsync(300, 400, text, clear, pressEnter);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TypeAsync(300, 400, text, clear, pressEnter), Times.Once);
        }

        [Fact]
        public async Task ClipboardTool_ClipboardAsync_CopyMode_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Text copied to clipboard";
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync("copy", "Test text"))
                               .ReturnsAsync(expectedResult);
            var clipboardTool = new ClipboardTool(_mockDesktopService.Object, _mockClipboardLogger.Object);

            // Act
            var result = await clipboardTool.ClipboardAsync("copy", "Test text");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClipboardOperationAsync("copy", "Test text"), Times.Once);
        }

        [Fact]
        public async Task ClipboardTool_ClipboardAsync_PasteMode_ShouldReturnClipboardContent()
        {
            // Arrange
            var expectedResult = "Clipboard content";
            _mockDesktopService.Setup(x => x.ClipboardOperationAsync("paste", null))
                               .ReturnsAsync(expectedResult);
            var clipboardTool = new ClipboardTool(_mockDesktopService.Object, _mockClipboardLogger.Object);

            // Act
            var result = await clipboardTool.ClipboardAsync("paste");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ClipboardOperationAsync("paste", null), Times.Once);
        }
    }

    /// <summary>
    /// 文件系统工具测试类
    /// </summary>
    public class FileSystemToolsTest
    {
        private readonly Mock<IFileSystemService> _mockFileSystemService;
        private readonly Mock<ILogger<ReadFileTool>> _mockReadLogger;
        private readonly Mock<ILogger<WriteFileTool>> _mockWriteLogger;

        public FileSystemToolsTest()
        {
            _mockFileSystemService = new Mock<IFileSystemService>();
            _mockReadLogger = new Mock<ILogger<ReadFileTool>>();
            _mockWriteLogger = new Mock<ILogger<WriteFileTool>>();
        }

        [Fact]
        public async Task ReadFileTool_ReadFileAsync_Success_ShouldReturnJsonWithContent()
        {
            // Arrange
            var filePath = "C:\\test.txt";
            var fileContent = "Test file content";
            _mockFileSystemService.Setup(x => x.ReadFileAsync(filePath))
                                 .ReturnsAsync((fileContent, 0));
            var readFileTool = new ReadFileTool(_mockFileSystemService.Object, _mockReadLogger.Object);

            // Act
            var result = await readFileTool.ReadFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(fileContent, jsonResult.GetProperty("content").GetString());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(fileContent.Length, jsonResult.GetProperty("contentLength").GetInt32());
            _mockFileSystemService.Verify(x => x.ReadFileAsync(filePath), Times.Once);
        }

        [Fact]
        public async Task ReadFileTool_ReadFileAsync_Failure_ShouldReturnJsonWithError()
        {
            // Arrange
            var filePath = "C:\\nonexistent.txt";
            var errorMessage = "File not found";
            _mockFileSystemService.Setup(x => x.ReadFileAsync(filePath))
                                 .ReturnsAsync((errorMessage, 1));
            var readFileTool = new ReadFileTool(_mockFileSystemService.Object, _mockReadLogger.Object);

            // Act
            var result = await readFileTool.ReadFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(errorMessage, jsonResult.GetProperty("message").GetString());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            _mockFileSystemService.Verify(x => x.ReadFileAsync(filePath), Times.Once);
        }

        [Fact]
        public async Task WriteFileTool_WriteFileAsync_Success_ShouldReturnJsonWithSuccess()
        {
            // Arrange
            var filePath = "C:\\output.txt";
            var content = "Content to write";
            var successMessage = "File written successfully";
            _mockFileSystemService.Setup(x => x.WriteFileAsync(filePath, content, false))
                                 .ReturnsAsync((successMessage, 0));
            var writeFileTool = new WriteFileTool(_mockFileSystemService.Object, _mockWriteLogger.Object);

            // Act
            var result = await writeFileTool.WriteFileAsync(filePath, content, false);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(successMessage, jsonResult.GetProperty("message").GetString());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(content.Length, jsonResult.GetProperty("contentLength").GetInt32());
            Assert.False(jsonResult.GetProperty("append").GetBoolean());
            _mockFileSystemService.Verify(x => x.WriteFileAsync(filePath, content, false), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WriteFileTool_WriteFileAsync_WithAppendMode_ShouldCallServiceWithCorrectParameters(bool append)
        {
            // Arrange
            var filePath = "C:\\test.txt";
            var content = "Test content";
            var successMessage = "Operation completed";
            _mockFileSystemService.Setup(x => x.WriteFileAsync(filePath, content, append))
                                 .ReturnsAsync((successMessage, 0));
            var writeFileTool = new WriteFileTool(_mockFileSystemService.Object, _mockWriteLogger.Object);

            // Act
            var result = await writeFileTool.WriteFileAsync(filePath, content, append);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(append, jsonResult.GetProperty("append").GetBoolean());
            _mockFileSystemService.Verify(x => x.WriteFileAsync(filePath, content, append), Times.Once);
        }

        [Fact]
        public async Task WriteFileTool_WriteFileAsync_Failure_ShouldReturnJsonWithError()
        {
            // Arrange
            var filePath = "C:\\readonly.txt";
            var content = "Content";
            var errorMessage = "Access denied";
            _mockFileSystemService.Setup(x => x.WriteFileAsync(filePath, content, false))
                                 .ReturnsAsync((errorMessage, 1));
            var writeFileTool = new WriteFileTool(_mockFileSystemService.Object, _mockWriteLogger.Object);

            // Act
            var result = await writeFileTool.WriteFileAsync(filePath, content, false);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(errorMessage, jsonResult.GetProperty("message").GetString());
            _mockFileSystemService.Verify(x => x.WriteFileAsync(filePath, content, false), Times.Once);
        }
    }

    /// <summary>
    /// OCR工具测试类
    /// </summary>
    public class OcrToolsTest
    {
        private readonly Mock<IOcrService> _mockOcrService;
        private readonly Mock<ILogger<ExtractTextFromScreenTool>> _mockExtractLogger;
        private readonly Mock<ILogger<FindTextOnScreenTool>> _mockFindLogger;

        public OcrToolsTest()
        {
            _mockOcrService = new Mock<IOcrService>();
            _mockExtractLogger = new Mock<ILogger<ExtractTextFromScreenTool>>();
            _mockFindLogger = new Mock<ILogger<FindTextOnScreenTool>>();
        }

        [Fact]
        public async Task ExtractTextFromScreenTool_ExtractTextFromScreenAsync_Success_ShouldReturnJsonWithText()
        {
            // Arrange
            var expectedResult = "Extracted screen text";
            _mockOcrService.Setup(x => x.ExtractTextFromScreenAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedResult, 0));
            var extractTool = new ExtractTextFromScreenTool(_mockOcrService.Object, _mockExtractLogger.Object);

            // Act
            var result = await extractTool.ExtractTextFromScreenAsync();

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("text").GetString());
            Assert.Equal("Text extracted successfully from screen", jsonResult.GetProperty("message").GetString());
            _mockOcrService.Verify(x => x.ExtractTextFromScreenAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExtractTextFromScreenTool_ExtractTextFromScreenAsync_Failure_ShouldReturnJsonWithError()
        {
            // Arrange
            _mockOcrService.Setup(x => x.ExtractTextFromScreenAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync((string.Empty, 1));
            var extractTool = new ExtractTextFromScreenTool(_mockOcrService.Object, _mockExtractLogger.Object);

            // Act
            var result = await extractTool.ExtractTextFromScreenAsync();

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(string.Empty, jsonResult.GetProperty("text").GetString());
            Assert.Equal("Failed to extract text from screen", jsonResult.GetProperty("message").GetString());
            _mockOcrService.Verify(x => x.ExtractTextFromScreenAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FindTextOnScreenTool_FindTextOnScreenAsync_Found_ShouldReturnJsonWithFoundTrue()
        {
            // Arrange
            var searchText = "Search text";
            _mockOcrService.Setup(x => x.FindTextOnScreenAsync(searchText, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((true, 0));
            var findTool = new FindTextOnScreenTool(_mockOcrService.Object, _mockFindLogger.Object);

            // Act
            var result = await findTool.FindTextOnScreenAsync(searchText);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.True(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal(searchText, jsonResult.GetProperty("searchText").GetString());
            Assert.Equal($"Text '{searchText}' found on screen", jsonResult.GetProperty("message").GetString());
            _mockOcrService.Verify(x => x.FindTextOnScreenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FindTextOnScreenTool_FindTextOnScreenAsync_NotFound_ShouldReturnJsonWithFoundFalse()
        {
            // Arrange
            var searchText = "Non-existent text";
            _mockOcrService.Setup(x => x.FindTextOnScreenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((false, 0));
            var findTool = new FindTextOnScreenTool(_mockOcrService.Object, _mockFindLogger.Object);

            // Act
            var result = await findTool.FindTextOnScreenAsync(searchText);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.False(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal(searchText, jsonResult.GetProperty("searchText").GetString());
            Assert.Equal($"Text '{searchText}' not found on screen", jsonResult.GetProperty("message").GetString());
            _mockOcrService.Verify(x => x.FindTextOnScreenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FindTextOnScreenTool_FindTextOnScreenAsync_ServiceFailure_ShouldReturnJsonWithError()
        {
            // Arrange
            var searchText = "Test text";
            _mockOcrService.Setup(x => x.FindTextOnScreenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((false, 1));
            var findTool = new FindTextOnScreenTool(_mockOcrService.Object, _mockFindLogger.Object);

            // Act
            var result = await findTool.FindTextOnScreenAsync(searchText);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.False(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal("Failed to search for text on screen", jsonResult.GetProperty("message").GetString());
            _mockOcrService.Verify(x => x.FindTextOnScreenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
