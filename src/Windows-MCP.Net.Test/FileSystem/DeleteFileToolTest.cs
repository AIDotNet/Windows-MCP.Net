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
    /// DeleteFileTool单元测试类
    /// </summary>
    public class DeleteFileToolTest
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<DeleteFileTool>> _mockLogger;

        public DeleteFileToolTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockLogger = new Mock<ILogger<DeleteFileTool>>();
        }

        [Fact]
        public async Task DeleteFileAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var filePath = "C:\\temp\\temp.txt";
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建要删除的文件
            File.WriteAllText(filePath, "This file will be deleted");
            
            var deleteFileTool = new DeleteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await deleteFileTool.DeleteFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            
            // 验证文件是否确实被删除
            Assert.False(File.Exists(filePath));
        }

        [Theory]
        [InlineData("test1.txt")]
        [InlineData("report.pdf")]
        [InlineData("image.jpg")]
        public async Task DeleteFileAsync_WithDifferentFiles_ShouldCallService(string fileName)
        {
            // Arrange
            var filePath = Path.Combine("C:\\temp", fileName);
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建要删除的文件
            File.WriteAllText(filePath, $"Content for {fileName}");
            
            var deleteFileTool = new DeleteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await deleteFileTool.DeleteFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            
            // 验证文件是否确实被删除
            Assert.False(File.Exists(filePath));
        }

        [Fact]
        public async Task DeleteFileAsync_WithNonExistentFile_ShouldReturnError()
        {
            // Arrange
            var filePath = "C:\\temp\\nonexistent.txt";
            var deleteFileTool = new DeleteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await deleteFileTool.DeleteFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
        }

        [Fact]
        public async Task DeleteFileAsync_WithInvalidPath_ShouldReturnError()
        {
            // Arrange
            var filePath = "Z:\\invalid\\path\\file.txt"; // 无效路径
            var deleteFileTool = new DeleteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await deleteFileTool.DeleteFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
        }

        [Fact]
        public async Task DeleteFileAsync_WithReadOnlyFile_ShouldHandle()
        {
            // Arrange
            var filePath = "C:\\temp\\readonly.txt";
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建只读文件
            File.WriteAllText(filePath, "Read only content");
            File.SetAttributes(filePath, FileAttributes.ReadOnly);
            
            var deleteFileTool = new DeleteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await deleteFileTool.DeleteFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            
            // 清理测试文件（如果仍然存在）
            if (File.Exists(filePath))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task DeleteFileAsync_WithDirectoryInsteadOfFile_ShouldReturnError()
        {
            // Arrange
            var directoryPath = "C:\\temp\\testdir";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directoryPath);
            
            var deleteFileTool = new DeleteFileTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await deleteFileTool.DeleteFileAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            
            // 清理测试目录
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }
    }
}
