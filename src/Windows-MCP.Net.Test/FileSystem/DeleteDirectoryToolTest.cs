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
    /// DeleteDirectoryTool单元测试类
    /// </summary>
    public class DeleteDirectoryToolTest
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<DeleteDirectoryTool>> _mockLogger;

        public DeleteDirectoryToolTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockLogger = new Mock<ILogger<DeleteDirectoryTool>>();
        }

        [Fact]
        public async Task DeleteDirectoryAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var directoryPath = "C:\\temp\\TempFolder";
            var recursive = false;
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建要删除的目录
            Directory.CreateDirectory(directoryPath);
            
            var deleteDirectoryTool = new DeleteDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await deleteDirectoryTool.DeleteDirectoryAsync(directoryPath, recursive);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.False(jsonResult.GetProperty("recursive").GetBoolean());
            
            // 验证目录是否确实被删除
            Assert.False(Directory.Exists(directoryPath));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteDirectoryAsync_WithRecursive_ShouldCallService(bool recursive)
        {
            // Arrange
            var directoryPath = "C:\\temp\\TestFolder";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建要删除的目录结构
            Directory.CreateDirectory(directoryPath);
            if (recursive)
            {
                // 为递归测试创建子目录和文件
                var subDir = Path.Combine(directoryPath, "SubFolder");
                Directory.CreateDirectory(subDir);
                File.WriteAllText(Path.Combine(directoryPath, "test.txt"), "test content");
                File.WriteAllText(Path.Combine(subDir, "subtest.txt"), "sub test content");
            }
            
            var deleteDirectoryTool = new DeleteDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await deleteDirectoryTool.DeleteDirectoryAsync(directoryPath, recursive);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(recursive, jsonResult.GetProperty("recursive").GetBoolean());
            
            // 验证目录是否确实被删除
            Assert.False(Directory.Exists(directoryPath));
        }

        [Fact]
        public async Task DeleteDirectoryAsync_WithNonRecursiveOnNonEmptyDirectory_ShouldFail()
        {
            // Arrange
            var directoryPath = "C:\\temp\\NonEmptyFolder";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建非空目录
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(Path.Combine(directoryPath, "file.txt"), "content");
            
            var deleteDirectoryTool = new DeleteDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await deleteDirectoryTool.DeleteDirectoryAsync(directoryPath, false);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.False(jsonResult.GetProperty("recursive").GetBoolean());
            
            // 验证目录仍然存在（因为非递归删除失败）
            Assert.True(Directory.Exists(directoryPath));
            
            // 清理测试目录
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        [Fact]
        public async Task DeleteDirectoryAsync_WithNonExistentDirectory_ShouldReturnError()
        {
            // Arrange
            var directoryPath = "C:\\temp\\NonExistentFolder";
            var deleteDirectoryTool = new DeleteDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await deleteDirectoryTool.DeleteDirectoryAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
        }

        [Fact]
        public async Task DeleteDirectoryAsync_WithInvalidPath_ShouldReturnError()
        {
            // Arrange
            var directoryPath = "Z:\\invalid\\path\\folder"; // 无效路径
            var deleteDirectoryTool = new DeleteDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await deleteDirectoryTool.DeleteDirectoryAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
        }
    }
}
