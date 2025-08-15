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
    /// CreateDirectoryTool单元测试类
    /// </summary>
    public class CreateDirectoryToolTest
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<CreateDirectoryTool>> _mockLogger;

        public CreateDirectoryToolTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockLogger = new Mock<ILogger<CreateDirectoryTool>>();
        }

        [Fact]
        public async Task CreateDirectoryAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var directoryPath = "C:\\temp\\NewFolder";
            
            // 确保父目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 确保目标目录不存在
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
            
            var createDirectoryTool = new CreateDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await createDirectoryTool.CreateDirectoryAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.True(jsonResult.GetProperty("createParents").GetBoolean());
            
            // 验证目录是否确实被创建
            Assert.True(Directory.Exists(directoryPath));
            
            // 清理测试目录
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateDirectoryAsync_WithCreateParents_ShouldCallService(bool createParents)
        {
            // Arrange
            var directoryPath = "C:\\temp\\Parent\\Child";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 清理可能存在的目录
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
            if (Directory.Exists("C:\\temp\\Parent"))
            {
                Directory.Delete("C:\\temp\\Parent", true);
            }
            
            var createDirectoryTool = new CreateDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await createDirectoryTool.CreateDirectoryAsync(directoryPath, createParents);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(createParents, jsonResult.GetProperty("createParents").GetBoolean());
            
            // 验证结果：当createParents为true时应该成功，为false时应该失败（因为父目录不存在）
            if (createParents)
            {
                Assert.True(jsonResult.GetProperty("success").GetBoolean());
                Assert.True(Directory.Exists(directoryPath));
            }
            else
            {
                // 当createParents为false且父目录不存在时，操作应该失败
                Assert.False(jsonResult.GetProperty("success").GetBoolean());
                Assert.False(Directory.Exists(directoryPath));
            }
            
            // 清理测试目录
            if (Directory.Exists("C:\\temp\\Parent"))
            {
                Directory.Delete("C:\\temp\\Parent", true);
            }
        }

        [Fact]
        public async Task CreateDirectoryAsync_WithExistingDirectory_ShouldReturnSuccess()
        {
            // Arrange
            var directoryPath = "C:\\temp\\ExistingFolder";
            
            // 确保目录存在
            Directory.CreateDirectory(directoryPath);
            
            var createDirectoryTool = new CreateDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await createDirectoryTool.CreateDirectoryAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            
            // 清理测试目录
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        [Fact]
        public async Task CreateDirectoryAsync_WithInvalidPath_ShouldReturnError()
        {
            // Arrange
            var directoryPath = "Z:\\invalid\\path\\folder"; // 无效路径
            var createDirectoryTool = new CreateDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await createDirectoryTool.CreateDirectoryAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
        }
    }
}
