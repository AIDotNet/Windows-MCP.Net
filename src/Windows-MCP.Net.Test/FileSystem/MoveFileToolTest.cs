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
    /// MoveFileTool单元测试类
    /// </summary>
    public class MoveFileToolTest
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<MoveFileTool>> _mockLogger;

        public MoveFileToolTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockLogger = new Mock<ILogger<MoveFileTool>>();
        }

        [Fact]
        public async Task MoveFileAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var source = "C:\\temp\\old.txt";
            var destination = "C:\\temp\\new.txt";
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            // 创建源文件
            File.WriteAllText(source, "Test content for move");
            // 确保目标文件不存在
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }
            
            var moveFileTool = new MoveFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await moveFileTool.MoveFileAsync(source, destination);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(source, jsonResult.GetProperty("source").GetString());
            Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            // 验证文件是否确实被移动
            Assert.False(File.Exists(source));
            Assert.True(File.Exists(destination));
            
            // 清理测试文件
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }
        }

        [Theory]
        [InlineData("test1.txt", "moved1.txt")]
        [InlineData("data.json", "archive-data.json")]
        public async Task MoveFileAsync_WithDifferentPaths_ShouldCallService(string sourceFile, string destFile)
        {
            // Arrange
            var source = Path.Combine("C:\\temp", sourceFile);
            var destination = Path.Combine("C:\\temp", destFile);
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建源文件
            File.WriteAllText(source, $"Test content for {sourceFile}");
            
            // 确保目标文件不存在
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }
            
            var moveFileTool = new MoveFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await moveFileTool.MoveFileAsync(source, destination);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(source, jsonResult.GetProperty("source").GetString());
            Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            
            // 验证文件是否确实被移动
            Assert.False(File.Exists(source));
            Assert.True(File.Exists(destination));
            
            // 清理测试文件
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task MoveFileAsync_WithOverwrite_ShouldCallService(bool overwrite)
        {
            // Arrange
            var source = "C:\\temp\\source.txt";
            var destination = "C:\\temp\\dest.txt";
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建源文件
            File.WriteAllText(source, "Source content");
            
            if (overwrite)
            {
                // 如果测试覆盖，先创建目标文件
                File.WriteAllText(destination, "Original destination content");
            }
            else
            {
                // 确保目标文件不存在
                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }
            }
            
            var moveFileTool = new MoveFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await moveFileTool.MoveFileAsync(source, destination, overwrite);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.Equal(source, jsonResult.GetProperty("source").GetString());
            Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            Assert.Equal(overwrite, jsonResult.GetProperty("overwrite").GetBoolean());
            
            if (overwrite || !File.Exists(destination))
            {
                Assert.True(jsonResult.GetProperty("success").GetBoolean());
                // 验证文件是否确实被移动
                Assert.False(File.Exists(source));
                Assert.True(File.Exists(destination));
            }
            
            // 清理测试文件
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }
            if (File.Exists(source))
            {
                File.Delete(source);
            }
        }

        [Fact]
        public async Task MoveFileAsync_WithNonExistentSource_ShouldReturnError()
        {
            // Arrange
            var source = "C:\\temp\\nonexistent.txt";
            var destination = "C:\\temp\\dest.txt";
            var moveFileTool = new MoveFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await moveFileTool.MoveFileAsync(source, destination);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(source, jsonResult.GetProperty("source").GetString());
            Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
        }

        [Fact]
        public async Task MoveFileAsync_WithInvalidDestinationPath_ShouldReturnError()
        {
            // Arrange
            var source = "C:\\temp\\test.txt";
            var destination = "Z:\\invalid\\path\\dest.txt"; // 无效路径
            
            // 创建源文件
            Directory.CreateDirectory("C:\\temp");
            File.WriteAllText(source, "Test content");
            
            var moveFileTool = new MoveFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await moveFileTool.MoveFileAsync(source, destination);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(source, jsonResult.GetProperty("source").GetString());
            Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            
            // 清理测试文件
            if (File.Exists(source))
            {
                File.Delete(source);
            }
        }

        [Fact]
        public async Task MoveFileAsync_ToSameLocation_ShouldHandleGracefully()
        {
            // Arrange
            var filePath = "C:\\temp\\samefile.txt";
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建文件
            File.WriteAllText(filePath, "Test content");
            
            var moveFileTool = new MoveFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await moveFileTool.MoveFileAsync(filePath, filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.Equal(filePath, jsonResult.GetProperty("source").GetString());
            Assert.Equal(filePath, jsonResult.GetProperty("destination").GetString());
            
            // 文件应该仍然存在
            Assert.True(File.Exists(filePath));
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
