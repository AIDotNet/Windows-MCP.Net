using Microsoft.Extensions.Logging;
using Moq;
using System.Drawing;
using System.Text.Json;
using WindowsMCP.Net.Tools.OCR;
using WindowsMCP.Net.Services;

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
    }
}