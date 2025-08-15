using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Tools.FileSystem;
using WindowsMCP.Net.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Windows_MCP.Net.Test.FileSystem
{
    /// <summary>
    /// ReadFileTool单元测试类
    /// </summary>
    public class ReadFileToolTest
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<ReadFileTool>> _mockLogger;

        public ReadFileToolTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockLogger = new Mock<ILogger<ReadFileTool>>();
        }

        [Fact]
        public async Task ReadFileAsync_ShouldReturnFileContent()
        {
            // Arrange
            var filePath = "C:\\temp\\test.txt";
            var testContent = "This is test content for reading";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建测试文件
            File.WriteAllText(filePath, testContent);
            var readFileTool = new ReadFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await readFileTool.ReadFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(testContent, jsonResult.GetProperty("content").GetString());
            Assert.Equal(testContent.Length, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Theory]
        [InlineData("test.txt", "Simple text content")]
        [InlineData("data.json", "{\"key\": \"value\", \"number\": 123}")]
        [InlineData("code.cs", "public class Test { public string Name { get; set; } }")]
        public async Task ReadFileAsync_WithDifferentContentTypes_ShouldReadCorrectly(string fileName, string content)
        {
            // Arrange
            var filePath = Path.Combine("C:\\temp", fileName);
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建测试文件
            File.WriteAllText(filePath, content);
            var readFileTool = new ReadFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await readFileTool.ReadFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(content, jsonResult.GetProperty("content").GetString());
            Assert.Equal(content.Length, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task ReadFileAsync_WithEmptyFile_ShouldReturnEmptyContent()
        {
            // Arrange
            var filePath = "C:\\temp\\empty.txt";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建空文件
            File.WriteAllText(filePath, "");
            var readFileTool = new ReadFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await readFileTool.ReadFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal("", jsonResult.GetProperty("content").GetString());
            Assert.Equal(0, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task ReadFileAsync_WithNonExistentFile_ShouldReturnError()
        {
            // Arrange
            var filePath = "C:\\temp\\nonexistent.txt";
            var readFileTool = new ReadFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await readFileTool.ReadFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.True(jsonResult.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.Null);
        }

        [Fact]
        public async Task ReadFileAsync_WithInvalidPath_ShouldReturnError()
        {
            // Arrange
            var filePath = "Z:\\invalid\\path\\file.txt"; // 无效路径
            var readFileTool = new ReadFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await readFileTool.ReadFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.True(jsonResult.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.Null);
        }

        [Fact]
        public async Task ReadFileAsync_WithLargeFile_ShouldReadCorrectly()
        {
            // Arrange
            var filePath = "C:\\temp\\large.txt";
            var largeContent = new string('A', 10000); // 10KB文件
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建大文件
            File.WriteAllText(filePath, largeContent);
            var readFileTool = new ReadFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await readFileTool.ReadFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(largeContent, jsonResult.GetProperty("content").GetString());
            Assert.Equal(largeContent.Length, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task ReadFileAsync_WithMultilineContent_ShouldPreserveFormat()
        {
            // Arrange
            var filePath = "C:\\temp\\multiline.txt";
            var multilineContent = "Line 1\nLine 2\nLine 3\n\nLine 5 with empty line above";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建多行文件
            File.WriteAllText(filePath, multilineContent);
            var readFileTool = new ReadFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await readFileTool.ReadFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(multilineContent, jsonResult.GetProperty("content").GetString());
            Assert.Equal(multilineContent.Length, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task ReadFileAsync_WithSpecialCharacters_ShouldReadCorrectly()
        {
            // Arrange
            var filePath = "C:\\temp\\special.txt";
            var specialContent = "Special chars: áéíóú ñ ü ß € ♠ ♣ ♥ ♦ 你好 こんにちは";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建包含特殊字符的文件
            File.WriteAllText(filePath, specialContent, System.Text.Encoding.UTF8);
            var readFileTool = new ReadFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await readFileTool.ReadFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(specialContent, jsonResult.GetProperty("content").GetString());
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
