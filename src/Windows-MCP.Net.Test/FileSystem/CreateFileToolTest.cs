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
    /// CreateFileTool单元测试类
    /// </summary>
    public class CreateFileToolTest
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<CreateFileTool>> _mockLogger;

        public CreateFileToolTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockLogger = new Mock<ILogger<CreateFileTool>>();
        }

        [Fact]
        public async Task CreateFileAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var filePath = "C:\\temp\\newfile.txt";
            var content = "Hello World";
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 确保文件不存在
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            var createFileTool = new CreateFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await createFileTool.CreateFileAsync(filePath, content);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(content.Length, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 验证文件是否确实被创建并包含正确内容
            Assert.True(File.Exists(filePath));
            var actualContent = File.ReadAllText(filePath);
            Assert.Equal(content, actualContent);
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Theory]
        [InlineData("test.txt", "")]
        [InlineData("data.json", "{\"key\": \"value\"}")]
        [InlineData("readme.md", "# Title\n\nContent")]
        public async Task CreateFileAsync_WithDifferentContent_ShouldCallService(string fileName, string content)
        {
            // Arrange
            var filePath = Path.Combine("C:\\temp", fileName);
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 确保文件不存在
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            var createFileTool = new CreateFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await createFileTool.CreateFileAsync(filePath, content);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(content.Length, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 验证文件是否确实被创建并包含正确内容
            Assert.True(File.Exists(filePath));
            var actualContent = File.ReadAllText(filePath);
            Assert.Equal(content, actualContent);
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task CreateFileAsync_WithDefaultEmptyContent_ShouldWork()
        {
            // Arrange
            var filePath = "C:\\temp\\emptyfile.txt";
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 确保文件不存在
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            var createFileTool = new CreateFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await createFileTool.CreateFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(0, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 验证文件是否确实被创建并且为空
            Assert.True(File.Exists(filePath));
            var actualContent = File.ReadAllText(filePath);
            Assert.Equal("", actualContent);
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task CreateFileAsync_WithInvalidPath_ShouldReturnError()
        {
            // Arrange
            var filePath = "Z:\\invalid\\path\\file.txt"; // 无效路径
            var content = "Test content";
            var createFileTool = new CreateFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await createFileTool.CreateFileAsync(filePath, content);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
        }

        [Fact]
        public async Task CreateFileAsync_WithExistingFile_ShouldOverwrite()
        {
            // Arrange
            var filePath = "C:\\temp\\existing.txt";
            var originalContent = "Original content";
            var newContent = "New content";
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建原文件
            File.WriteAllText(filePath, originalContent);
            
            var createFileTool = new CreateFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await createFileTool.CreateFileAsync(filePath, newContent);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(newContent.Length, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 验证文件内容被覆盖
            var actualContent = File.ReadAllText(filePath);
            Assert.Equal(newContent, actualContent);
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
