using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using WindowsMCP.Net.Tools.OCR;
using Interface;
using WindowsMCP.Net.Services;
using System.Drawing;
using System.Drawing.Imaging;

namespace Windows_MCP.Net.Test.OCR
{
    /// <summary>
    /// ExtractTextFromFileTool 的专用单元测试类
    /// 测试从图像文件提取文本的所有功能
    /// </summary>
    public class ExtractTextFromFileToolTest
    {
        private readonly IOcrService _ocrService;
        private readonly ILogger<ExtractTextFromFileTool> _logger;
        private readonly ExtractTextFromFileTool _tool;
        private readonly string _testImageDirectory;

        public ExtractTextFromFileToolTest()
        {
            _ocrService = new OcrService();
            _logger = NullLogger<ExtractTextFromFileTool>.Instance;
            _tool = new ExtractTextFromFileTool(_ocrService, _logger);
            _testImageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestImages");
            
            // 确保测试图像目录存在
            if (!Directory.Exists(_testImageDirectory))
            {
                Directory.CreateDirectory(_testImageDirectory);
            }
        }

        #region 基础功能测试

        /// <summary>
        /// 测试从有效图像文件提取文字的基本功能
        /// </summary>
        [Fact]
        public async Task ExtractTextFromFileAsync_ValidImageFile_ShouldReturnSuccess()
        {
            // Arrange
            var testImagePath = CreateTestImageWithText("Hello World Test");

            try
            {
                // Act
                var result = await _tool.ExtractTextFromFileAsync(testImagePath);

                // Assert
                Assert.NotNull(result);
                Assert.NotEmpty(result);

                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                Assert.True(jsonResult.GetProperty("success").GetBoolean());
                Assert.True(jsonResult.GetProperty("text").GetString()?.Length > 0);
                Assert.Equal(testImagePath, jsonResult.GetProperty("filePath").GetString());
            }
            finally
            {
                // Cleanup
                if (File.Exists(testImagePath))
                {
                    File.Delete(testImagePath);
                }
            }
        }

        /// <summary>
        /// 测试文件不存在时的错误处理
        /// </summary>
        [Fact]
        public async Task ExtractTextFromFileAsync_FileNotExists_ShouldReturnError()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testImageDirectory, "nonexistent.png");

            // Act
            var result = await _tool.ExtractTextFromFileAsync(nonExistentPath);

            // Assert
            Assert.NotNull(result);
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Contains("File not found", jsonResult.GetProperty("message").GetString());
            Assert.Equal(string.Empty, jsonResult.GetProperty("text").GetString());
        }

        /// <summary>
        /// 测试不支持的文件格式
        /// </summary>
        [Fact]
        public async Task ExtractTextFromFileAsync_UnsupportedFormat_ShouldReturnError()
        {
            // Arrange
            var textFilePath = Path.Combine(_testImageDirectory, "test.txt");
            await File.WriteAllTextAsync(textFilePath, "This is a text file");

            try
            {
                // Act
                var result = await _tool.ExtractTextFromFileAsync(textFilePath);

                // Assert
                Assert.NotNull(result);
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                Assert.False(jsonResult.GetProperty("success").GetBoolean());
                Assert.Contains("Unsupported file format", jsonResult.GetProperty("message").GetString());
                Assert.Equal(string.Empty, jsonResult.GetProperty("text").GetString());
            }
            finally
            {
                // Cleanup
                if (File.Exists(textFilePath))
                {
                    File.Delete(textFilePath);
                }
            }
        }

        #endregion

        #region 支持的文件格式测试

        /// <summary>
        /// 测试支持的图像格式
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        [Theory]
        [InlineData(".png")]
        [InlineData(".jpg")]
        [InlineData(".jpeg")]
        [InlineData(".bmp")]
        public async Task ExtractTextFromFileAsync_SupportedFormats_ShouldWork(string extension)
        {
            // Arrange
            var testImagePath = CreateTestImageWithText("Format Test", extension);

            try
            {
                // Act
                var result = await _tool.ExtractTextFromFileAsync(testImagePath);

                // Assert
                Assert.NotNull(result);
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                // 注意：由于是简单的测试图像，OCR可能无法识别文字，所以只测试不会出错
                Assert.True(jsonResult.TryGetProperty("success", out _));
                Assert.True(jsonResult.TryGetProperty("text", out _));
                Assert.True(jsonResult.TryGetProperty("message", out _));
            }
            finally
            {
                // Cleanup
                if (File.Exists(testImagePath))
                {
                    File.Delete(testImagePath);
                }
            }
        }

        #endregion

        #region 错误处理测试

        /// <summary>
        /// 测试空路径参数
        /// </summary>
        [Fact]
        public async Task ExtractTextFromFileAsync_EmptyPath_ShouldReturnError()
        {
            // Act
            var result = await _tool.ExtractTextFromFileAsync(string.Empty);

            // Assert
            Assert.NotNull(result);
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
        }

        /// <summary>
        /// 测试null路径参数
        /// </summary>
        [Fact]
        public async Task ExtractTextFromFileAsync_NullPath_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _tool.ExtractTextFromFileAsync(null));
        }

        /// <summary>
        /// 测试无效路径字符
        /// </summary>
        [Fact]
        public async Task ExtractTextFromFileAsync_InvalidPathCharacters_ShouldReturnError()
        {
            // Arrange
            var invalidPath = "C:\\invalid<>path.png";

            // Act
            var result = await _tool.ExtractTextFromFileAsync(invalidPath);

            // Assert
            Assert.NotNull(result);
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建包含文字的测试图像
        /// </summary>
        /// <param name="text">要包含的文字</param>
        /// <param name="extension">文件扩展名</param>
        /// <returns>创建的图像文件路径</returns>
        private string CreateTestImageWithText(string text, string extension = ".png")
        {
            var fileName = $"test_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_testImageDirectory, fileName);

            // 创建一个简单的图像，包含文字
            using (var bitmap = new Bitmap(400, 200))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // 设置背景为白色
                graphics.Clear(Color.White);

                // 绘制黑色文字
                using (var font = new Font("Arial", 24, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.Black))
                {
                    graphics.DrawString(text, font, brush, new PointF(50, 80));
                }

                // 根据扩展名保存为相应格式
                var format = extension.ToLowerInvariant() switch
                {
                    ".png" => ImageFormat.Png,
                    ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                    ".bmp" => ImageFormat.Bmp,
                    ".gif" => ImageFormat.Gif,
                    ".tiff" or ".tif" => ImageFormat.Tiff,
                    _ => ImageFormat.Png
                };

                bitmap.Save(filePath, format);
            }

            return filePath;
        }

        /// <summary>
        /// 创建空的测试图像
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        /// <returns>创建的图像文件路径</returns>
        private string CreateEmptyTestImage(string extension = ".png")
        {
            var fileName = $"empty_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_testImageDirectory, fileName);

            using (var bitmap = new Bitmap(100, 100))
            {
                bitmap.Save(filePath, ImageFormat.Png);
            }

            return filePath;
        }

        #endregion

        #region 性能测试

        /// <summary>
        /// 测试处理大图像文件的性能
        /// </summary>
        [Fact]
        public async Task ExtractTextFromFileAsync_LargeImage_ShouldCompleteInReasonableTime()
        {
            // Arrange
            var largeImagePath = CreateLargeTestImage();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Act
                var result = await _tool.ExtractTextFromFileAsync(largeImagePath);
                stopwatch.Stop();

                // Assert
                Assert.NotNull(result);
                Assert.True(stopwatch.ElapsedMilliseconds < 30000); // 应该在30秒内完成
                
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                Assert.True(jsonResult.TryGetProperty("success", out _));
            }
            finally
            {
                // Cleanup
                if (File.Exists(largeImagePath))
                {
                    File.Delete(largeImagePath);
                }
            }
        }

        /// <summary>
        /// 创建大尺寸测试图像
        /// </summary>
        /// <returns>创建的大图像文件路径</returns>
        private string CreateLargeTestImage()
        {
            var fileName = $"large_test_{Guid.NewGuid()}.png";
            var filePath = Path.Combine(_testImageDirectory, fileName);

            using (var bitmap = new Bitmap(2000, 1500))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                
                using (var font = new Font("Arial", 36, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.Black))
                {
                    graphics.DrawString("Large Image Test", font, brush, new PointF(100, 200));
                    graphics.DrawString("Performance Testing", font, brush, new PointF(100, 300));
                    graphics.DrawString("OCR Processing", font, brush, new PointF(100, 400));
                }

                bitmap.Save(filePath, ImageFormat.Png);
            }

            return filePath;
        }

        #endregion
    }
}