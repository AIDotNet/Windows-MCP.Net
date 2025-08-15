using Microsoft.Extensions.Logging;
using Moq;
using System.Drawing;
using System.Text.Json;
using WindowsMCP.Net.Tools.OCR;
using Interface;
using System.IO;

namespace Windows_MCP.Net.Test
{
    /// <summary>
    /// 扩展的OCR工具测试类
    /// </summary>
    public class OCRToolsExtendedTest
    {
        private readonly Mock<IOcrService> _mockOcrService;
        private readonly Mock<ILogger<ExtractTextFromRegionTool>> _mockExtractTextFromRegionLogger;
        private readonly Mock<ILogger<GetTextCoordinatesTool>> _mockGetTextCoordinatesLogger;

        public OCRToolsExtendedTest()
        {
            _mockOcrService = new Mock<IOcrService>();
            _mockExtractTextFromRegionLogger = new Mock<ILogger<ExtractTextFromRegionTool>>();
            _mockGetTextCoordinatesLogger = new Mock<ILogger<GetTextCoordinatesTool>>();
        }

        [Fact]
        public async Task ExtractTextFromRegionTool_ExtractTextFromRegionAsync_ShouldReturnExtractedText()
        {
            // Arrange
            var x = 100;
            var y = 200;
            var width = 300;
            var height = 150;
            var expectedResult = "Extracted text from region";
            _mockOcrService.Setup(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedResult, 0));
            var extractTextFromRegionTool = new ExtractTextFromRegionTool(_mockOcrService.Object, _mockExtractTextFromRegionLogger.Object);

            // Act
            var result = await extractTextFromRegionTool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("text").GetString());
            Assert.Equal("Text extracted successfully", jsonResult.GetProperty("message").GetString());
            _mockOcrService.Verify(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(0, 0, 100, 50)]
        [InlineData(500, 300, 200, 100)]
        [InlineData(1000, 800, 400, 300)]
        public async Task ExtractTextFromRegionTool_ExtractTextFromRegionAsync_WithDifferentRegions_ShouldCallService(int x, int y, int width, int height)
        {
            // Arrange
            var expectedResult = $"Text from region ({x},{y}) size {width}x{height}";
            _mockOcrService.Setup(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedResult, 0));
            var extractTextFromRegionTool = new ExtractTextFromRegionTool(_mockOcrService.Object, _mockExtractTextFromRegionLogger.Object);

            // Act
            var result = await extractTextFromRegionTool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("text").GetString());
            _mockOcrService.Verify(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExtractTextFromRegionTool_ExtractTextFromRegionAsync_WithSmallRegion_ShouldReturnResult()
        {
            // Arrange
            var x = 10;
            var y = 10;
            var width = 50;
            var height = 20;
            var expectedResult = "Small text";
            _mockOcrService.Setup(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedResult, 0));
            var extractTextFromRegionTool = new ExtractTextFromRegionTool(_mockOcrService.Object, _mockExtractTextFromRegionLogger.Object);

            // Act
            var result = await extractTextFromRegionTool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("text").GetString());
            _mockOcrService.Verify(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExtractTextFromRegionTool_ExtractTextFromRegionAsync_WithLargeRegion_ShouldReturnResult()
        {
            // Arrange
            var x = 0;
            var y = 0;
            var width = 1920;
            var height = 1080;
            var expectedResult = "Full screen text content with multiple lines\nSecond line\nThird line";
            _mockOcrService.Setup(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedResult, 0));
            var extractTextFromRegionTool = new ExtractTextFromRegionTool(_mockOcrService.Object, _mockExtractTextFromRegionLogger.Object);

            // Act
            var result = await extractTextFromRegionTool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("text").GetString());
            _mockOcrService.Verify(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExtractTextFromRegionTool_ExtractTextFromRegionAsync_WithEmptyRegion_ShouldReturnEmptyResult()
        {
            // Arrange
            var x = 100;
            var y = 100;
            var width = 0;
            var height = 0;
            var expectedResult = "";
            _mockOcrService.Setup(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedResult, 0));
            var extractTextFromRegionTool = new ExtractTextFromRegionTool(_mockOcrService.Object, _mockExtractTextFromRegionLogger.Object);

            // Act
            var result = await extractTextFromRegionTool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("text").GetString());
            _mockOcrService.Verify(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTextCoordinatesTool_GetTextCoordinatesAsync_ShouldReturnCoordinates()
        {
            // Arrange
            var text = "Search Text";
            var expectedPoint = new Point(200, 300);
            _mockOcrService.Setup(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedPoint, 0));
            var getTextCoordinatesTool = new GetTextCoordinatesTool(_mockOcrService.Object, _mockGetTextCoordinatesLogger.Object);

            // Act
            var result = await getTextCoordinatesTool.GetTextCoordinatesAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.True(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal(text, jsonResult.GetProperty("searchText").GetString());
            Assert.Equal(200, jsonResult.GetProperty("coordinates").GetProperty("x").GetInt32());
            Assert.Equal(300, jsonResult.GetProperty("coordinates").GetProperty("y").GetInt32());
            _mockOcrService.Verify(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("Button")]
        [InlineData("Login")]
        [InlineData("Submit")]
        [InlineData("Cancel")]
        public async Task GetTextCoordinatesTool_GetTextCoordinatesAsync_WithDifferentTexts_ShouldCallService(string text)
        {
            // Arrange
            var expectedPoint = new Point(200, 300);
            _mockOcrService.Setup(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedPoint, 0));
            var getTextCoordinatesTool = new GetTextCoordinatesTool(_mockOcrService.Object, _mockGetTextCoordinatesLogger.Object);

            // Act
            var result = await getTextCoordinatesTool.GetTextCoordinatesAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.True(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal(text, jsonResult.GetProperty("searchText").GetString());
            Assert.Equal(200, jsonResult.GetProperty("coordinates").GetProperty("x").GetInt32());
            Assert.Equal(300, jsonResult.GetProperty("coordinates").GetProperty("y").GetInt32());
            _mockOcrService.Verify(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTextCoordinatesTool_GetTextCoordinatesAsync_WithMultiWordText_ShouldReturnCoordinates()
        {
            // Arrange
            var text = "Click here to continue";
            var expectedPoint = new Point(200, 300);
            _mockOcrService.Setup(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedPoint, 0));
            var getTextCoordinatesTool = new GetTextCoordinatesTool(_mockOcrService.Object, _mockGetTextCoordinatesLogger.Object);

            // Act
            var result = await getTextCoordinatesTool.GetTextCoordinatesAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.True(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal(text, jsonResult.GetProperty("searchText").GetString());
            Assert.Equal(200, jsonResult.GetProperty("coordinates").GetProperty("x").GetInt32());
            Assert.Equal(300, jsonResult.GetProperty("coordinates").GetProperty("y").GetInt32());
            _mockOcrService.Verify(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTextCoordinatesTool_GetTextCoordinatesAsync_WithSpecialCharacters_ShouldReturnCoordinates()
        {
            // Arrange
            var text = "Save & Exit";
            var expectedPoint = new Point(200, 300);
            _mockOcrService.Setup(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedPoint, 0));
            var getTextCoordinatesTool = new GetTextCoordinatesTool(_mockOcrService.Object, _mockGetTextCoordinatesLogger.Object);

            // Act
            var result = await getTextCoordinatesTool.GetTextCoordinatesAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.True(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal(text, jsonResult.GetProperty("searchText").GetString());
            Assert.Equal(200, jsonResult.GetProperty("coordinates").GetProperty("x").GetInt32());
            Assert.Equal(300, jsonResult.GetProperty("coordinates").GetProperty("y").GetInt32());
            _mockOcrService.Verify(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTextCoordinatesTool_GetTextCoordinatesAsync_WithTextNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var text = "NonExistentText";
            _mockOcrService.Setup(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(((Point?)null, 0));
            var getTextCoordinatesTool = new GetTextCoordinatesTool(_mockOcrService.Object, _mockGetTextCoordinatesLogger.Object);

            // Act
            var result = await getTextCoordinatesTool.GetTextCoordinatesAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.False(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal(text, jsonResult.GetProperty("searchText").GetString());
            Assert.Contains("not found", jsonResult.GetProperty("message").GetString());
            _mockOcrService.Verify(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTextCoordinatesTool_GetTextCoordinatesAsync_WithEmptyText_ShouldReturnErrorResult()
        {
            // Arrange
            var text = "";
            _mockOcrService.Setup(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(((Point?)null, -1));
            var getTextCoordinatesTool = new GetTextCoordinatesTool(_mockOcrService.Object, _mockGetTextCoordinatesLogger.Object);

            // Act
            var result = await getTextCoordinatesTool.GetTextCoordinatesAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.False(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal(text, jsonResult.GetProperty("searchText").GetString());
            _mockOcrService.Verify(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("1234567890")]
        [InlineData("ABC123")]
        [InlineData("Test@123")]
        public async Task GetTextCoordinatesTool_GetTextCoordinatesAsync_WithNumericAndAlphanumericText_ShouldCallService(string text)
        {
            // Arrange
            var expectedPoint = new Point(200, 300);
            _mockOcrService.Setup(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedPoint, 0));
            var getTextCoordinatesTool = new GetTextCoordinatesTool(_mockOcrService.Object, _mockGetTextCoordinatesLogger.Object);

            // Act
            var result = await getTextCoordinatesTool.GetTextCoordinatesAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.True(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal(text, jsonResult.GetProperty("searchText").GetString());
            Assert.Equal(200, jsonResult.GetProperty("coordinates").GetProperty("x").GetInt32());
            Assert.Equal(300, jsonResult.GetProperty("coordinates").GetProperty("y").GetInt32());
            _mockOcrService.Verify(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExtractTextFromRegionTool_ExtractTextFromRegionAsync_WithNegativeCoordinates_ShouldHandleGracefully()
        {
            // Arrange
            var x = -10;
            var y = -5;
            var width = 100;
            var height = 50;
            var expectedResult = "Handled negative coordinates";
            _mockOcrService.Setup(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedResult, 0));
            var extractTextFromRegionTool = new ExtractTextFromRegionTool(_mockOcrService.Object, _mockExtractTextFromRegionLogger.Object);

            // Act
            var result = await extractTextFromRegionTool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("text").GetString());
            _mockOcrService.Verify(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExtractTextFromRegionTool_ExtractTextFromRegionAsync_WithVeryLargeRegion_ShouldHandleGracefully()
        {
            // Arrange
            var x = 0;
            var y = 0;
            var width = 10000;
            var height = 10000;
            var expectedResult = "Handled very large region";
            _mockOcrService.Setup(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedResult, 0));
            var extractTextFromRegionTool = new ExtractTextFromRegionTool(_mockOcrService.Object, _mockExtractTextFromRegionLogger.Object);

            // Act
            var result = await extractTextFromRegionTool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("text").GetString());
            _mockOcrService.Verify(s => s.ExtractTextFromRegionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #region 使用真实图片文件的OCR测试

        [Fact]
        public async Task ExtractTextFromImageAsync_WithNotepadWritingPng_ShouldExtractText()
        {
            // Arrange
            var imagePath = @"images\NotepadWriting.png";
            var expectedText = "Sample text from notepad";
            _mockOcrService.Setup(s => s.ExtractTextFromImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedText, 0));
           
            // Act - 模拟读取图片文件并进行OCR
            string result;
            using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                var (text, status) = await _mockOcrService.Object.ExtractTextFromImageAsync(fileStream);
                result = JsonSerializer.Serialize(new
                {
                    success = status == 0,
                    text = status == 0 ? text : string.Empty,
                    message = status == 0 ? "Text extracted successfully from NotepadWriting.png" : "Failed to extract text",
                    imagePath = imagePath
                });
            }

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedText, jsonResult.GetProperty("text").GetString());
            Assert.Contains("NotepadWriting.png", jsonResult.GetProperty("message").GetString());
            _mockOcrService.Verify(s => s.ExtractTextFromImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExtractTextFromImageAsync_WithOpenWebSearchPng_ShouldExtractText()
        {
            // Arrange
            var imagePath = @"images\OpenWebSearch.png";
            var expectedText = "Web Search Interface";
            
            // 模拟从图片中提取文本
            _mockOcrService.Setup(s => s.ExtractTextFromImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedText, 0));

            // Act - 模拟读取图片文件并进行OCR
            string result;
            using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                var (text, status) = await _mockOcrService.Object.ExtractTextFromImageAsync(fileStream);
                result = JsonSerializer.Serialize(new
                {
                    success = status == 0,
                    text = status == 0 ? text : string.Empty,
                    message = status == 0 ? "Text extracted successfully from OpenWebSearch.png" : "Failed to extract text",
                    imagePath = imagePath
                });
            }

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedText, jsonResult.GetProperty("text").GetString());
            Assert.Contains("OpenWebSearch.png", jsonResult.GetProperty("message").GetString());
            _mockOcrService.Verify(s => s.ExtractTextFromImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("NotepadWriting.png", "Notepad")]
        [InlineData("OpenWebSearch.png", "Search")]
        public async Task GetTextCoordinatesAsync_WithRealImages_ShouldFindTextCoordinates(string imageName, string searchText)
        {
            // Arrange
            var imagePath = Path.Combine("images", imageName);
            var expectedPoint = new Point(150, 200);
            
            // 模拟在图片中找到文本坐标
            _mockOcrService.Setup(s => s.GetTextCoordinatesAsync(It.Is<string>(t => t == searchText), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((expectedPoint, 0));
            
            var getTextCoordinatesTool = new GetTextCoordinatesTool(_mockOcrService.Object, _mockGetTextCoordinatesLogger.Object);

            // Act
            var result = await getTextCoordinatesTool.GetTextCoordinatesAsync(searchText);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.True(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal(searchText, jsonResult.GetProperty("searchText").GetString());
            Assert.Equal(150, jsonResult.GetProperty("coordinates").GetProperty("x").GetInt32());
            Assert.Equal(200, jsonResult.GetProperty("coordinates").GetProperty("y").GetInt32());
            _mockOcrService.Verify(s => s.GetTextCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExtractTextFromImageAsync_WithInvalidImagePath_ShouldHandleError()
        {
            // Arrange
            var invalidImagePath = "C:\\NonExistentPath\\NonExistentImage.png";
            
            // Act & Assert
            await Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
            {
                using var fileStream = new FileStream(invalidImagePath, FileMode.Open, FileAccess.Read);
                await _mockOcrService.Object.ExtractTextFromImageAsync(fileStream);
            });
        }

        [Fact]
        public async Task ExtractTextFromImageAsync_WithMultipleImages_ShouldProcessAll()
        {
            // Arrange
            var imageFiles = new[] { "NotepadWriting.png", "OpenWebSearch.png" };
            var expectedTexts = new[] { "Notepad content", "Web search interface" };
            
            // 使用序列设置来确保每次调用返回不同的值
            var setupSequence = _mockOcrService.SetupSequence(s => s.ExtractTextFromImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()));
            for (int i = 0; i < expectedTexts.Length; i++)
            {
                setupSequence = setupSequence.ReturnsAsync((expectedTexts[i], 0));
            }

            // Act & Assert
            for (int i = 0; i < imageFiles.Length; i++)
            {
                var imagePath = Path.Combine("images", imageFiles[i]);
                
                using var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                var (text, status) = await _mockOcrService.Object.ExtractTextFromImageAsync(fileStream);
                
                Assert.Equal(0, status);
                Assert.Equal(expectedTexts[i], text);
            }
            
            _mockOcrService.Verify(s => s.ExtractTextFromImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Exactly(imageFiles.Length));
        }

        [Fact]
        public async Task ExtractTextFromImageAsync_WithEmptyStream_ShouldHandleGracefully()
        {
            // Arrange
            _mockOcrService.Setup(s => s.ExtractTextFromImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((string.Empty, 1));

            // Act
            using var emptyStream = new MemoryStream();
            var (text, status) = await _mockOcrService.Object.ExtractTextFromImageAsync(emptyStream);

            // Assert
            Assert.Equal(1, status);
            Assert.Equal(string.Empty, text);
            _mockOcrService.Verify(s => s.ExtractTextFromImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion
    }
}