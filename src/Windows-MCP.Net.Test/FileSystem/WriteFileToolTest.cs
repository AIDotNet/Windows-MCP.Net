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
    /// WriteFileTool单元测试类
    /// </summary>
    public class WriteFileToolTest
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<WriteFileTool>> _mockLogger;

        public WriteFileToolTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockLogger = new Mock<ILogger<WriteFileTool>>();
        }

        [Fact]
        public async Task WriteFileAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var filePath = "C:\\temp\\write_test.txt";
            var content = "This is test content for writing";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 确保文件不存在
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            var writeFileTool = new WriteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await writeFileTool.WriteFileAsync(filePath, content);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(content.Length, jsonResult.GetProperty("contentLength").GetInt32());
            Assert.False(jsonResult.GetProperty("append").GetBoolean());
            
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
        [InlineData("simple.txt", "Simple content")]
        [InlineData("data.json", "{\"key\": \"value\", \"array\": [1,2,3]}")]
        [InlineData("code.cs", "public class Test\n{\n    public string Name { get; set; }\n}")]
        public async Task WriteFileAsync_WithDifferentContent_ShouldWork(string fileName, string content)
        {
            // Arrange
            var filePath = Path.Combine("C:\\temp", fileName);
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 确保文件不存在
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            var writeFileTool = new WriteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await writeFileTool.WriteFileAsync(filePath, content);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(content.Length, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 验证文件内容
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
        [InlineData(true)]
        [InlineData(false)]
        public async Task WriteFileAsync_WithAppendOption_ShouldWork(bool append)
        {
            // Arrange
            var filePath = "C:\\temp\\append_test.txt";
            var originalContent = "Original content";
            var newContent = " Additional content";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 先创建原文件
            File.WriteAllText(filePath, originalContent);
            
            var writeFileTool = new WriteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await writeFileTool.WriteFileAsync(filePath, newContent, append);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(newContent.Length, jsonResult.GetProperty("contentLength").GetInt32());
            Assert.Equal(append, jsonResult.GetProperty("append").GetBoolean());
            
            // 验证文件内容
            Assert.True(File.Exists(filePath));
            var actualContent = File.ReadAllText(filePath);
            
            if (append)
            {
                Assert.Equal(originalContent + newContent, actualContent);
            }
            else
            {
                Assert.Equal(newContent, actualContent);
            }
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task WriteFileAsync_WithEmptyContent_ShouldWork()
        {
            // Arrange
            var filePath = "C:\\temp\\empty_write.txt";
            var content = "";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 确保文件不存在
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            var writeFileTool = new WriteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await writeFileTool.WriteFileAsync(filePath, content);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(0, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 验证文件被创建但为空
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
        public async Task WriteFileAsync_WithInvalidPath_ShouldReturnError()
        {
            // Arrange
            var filePath = "Z:\\invalid\\path\\file.txt"; // 无效路径
            var content = "Test content";
            var writeFileTool = new WriteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await writeFileTool.WriteFileAsync(filePath, content);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
        }

        [Fact]
        public async Task WriteFileAsync_WithLargeContent_ShouldWork()
        {
            // Arrange
            var filePath = "C:\\temp\\large_write.txt";
            var largeContent = new string('X', 50000); // 50KB内容
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 确保文件不存在
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            var writeFileTool = new WriteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await writeFileTool.WriteFileAsync(filePath, largeContent);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(largeContent.Length, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 验证文件内容
            Assert.True(File.Exists(filePath));
            var actualContent = File.ReadAllText(filePath);
            Assert.Equal(largeContent, actualContent);
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task WriteFileAsync_WithSpecialCharacters_ShouldWork()
        {
            // Arrange
            var filePath = "C:\\temp\\special_write.txt";
            var specialContent = "Special chars: áéíóú ñ ü ß € ♠ ♣ ♥ ♦ 你好 こんにちは\nNew line test";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 确保文件不存在
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            var writeFileTool = new WriteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await writeFileTool.WriteFileAsync(filePath, specialContent);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(specialContent.Length, jsonResult.GetProperty("contentLength").GetInt32());
            
            // 验证文件内容（使用UTF-8读取）
            Assert.True(File.Exists(filePath));
            var actualContent = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            Assert.Equal(specialContent, actualContent);
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task WriteFileAsync_OverwriteExistingFile_ShouldWork()
        {
            // Arrange
            var filePath = "C:\\temp\\overwrite_test.txt";
            var originalContent = "Original content";
            var newContent = "New overwritten content";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建原文件
            File.WriteAllText(filePath, originalContent);
            
            var writeFileTool = new WriteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await writeFileTool.WriteFileAsync(filePath, newContent, false);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(newContent.Length, jsonResult.GetProperty("contentLength").GetInt32());
            Assert.False(jsonResult.GetProperty("append").GetBoolean());
            
            // 验证文件内容被覆盖
            var actualContent = File.ReadAllText(filePath);
            Assert.Equal(newContent, actualContent);
            Assert.NotEqual(originalContent, actualContent);
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
