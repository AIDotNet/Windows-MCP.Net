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
    [Collection("FileSystemTests")]
    public class MoveFileToolTest : IDisposable
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<MoveFileTool>> _mockLogger;
        private readonly string _testDirectory;
        private readonly List<string> _filesToCleanup;

        public MoveFileToolTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockLogger = new Mock<ILogger<MoveFileTool>>();
            
            // 使用临时目录和唯一标识符创建测试环境
            _testDirectory = Path.Combine(Path.GetTempPath(), "MoveFileToolTest", Guid.NewGuid().ToString());
            _filesToCleanup = new List<string>();
            
            // 确保测试目录存在
            Directory.CreateDirectory(_testDirectory);
        }

        private string GetUniqueFilePath(string fileName)
        {
            var uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            var filePath = Path.Combine(_testDirectory, uniqueFileName);
            _filesToCleanup.Add(filePath);
            return filePath;
        }

        private void CleanupFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // 忽略清理错误
            }
        }

        [Fact]
        public async Task MoveFileAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var source = GetUniqueFilePath("old.txt");
            var destination = GetUniqueFilePath("new.txt");
            
            try
            {
                // 创建源文件
                File.WriteAllText(source, "Test content for move");
                
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
            }
            finally
            {
                // 确保资源清理
                CleanupFile(source);
                CleanupFile(destination);
            }
        }

        [Theory]
        [InlineData("test1.txt", "moved1.txt")]
        [InlineData("data.json", "archive-data.json")]
        public async Task MoveFileAsync_WithDifferentPaths_ShouldCallService(string sourceFile, string destFile)
        {
            // Arrange
            var source = GetUniqueFilePath(sourceFile);
            var destination = GetUniqueFilePath(destFile);
            
            try
            {
                // 创建源文件
                File.WriteAllText(source, $"Test content for {sourceFile}");
                
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
            }
            finally
            {
                // 确保资源清理
                CleanupFile(source);
                CleanupFile(destination);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task MoveFileAsync_WithOverwrite_ShouldCallService(bool overwrite)
        {
            // Arrange
            var source = GetUniqueFilePath("source.txt");
            var destination = GetUniqueFilePath("dest.txt");
            
            try
            {
                // 创建源文件
                File.WriteAllText(source, "Source content");
                
                if (overwrite)
                {
                    // 如果测试覆盖，先创建目标文件
                    File.WriteAllText(destination, "Original destination content");
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
            }
            finally
            {
                // 确保资源清理
                CleanupFile(source);
                CleanupFile(destination);
            }
        }

        [Fact]
        public async Task MoveFileAsync_WithNonExistentSource_ShouldReturnError()
        {
            // Arrange
            var source = GetUniqueFilePath("nonexistent.txt");
            var destination = GetUniqueFilePath("dest.txt");
            var moveFileTool = new MoveFileTool(_fileSystemService, _mockLogger.Object);

            try
            {
                // Act
                var result = await moveFileTool.MoveFileAsync(source, destination);

                // Assert
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                Assert.False(jsonResult.GetProperty("success").GetBoolean());
                Assert.Equal(source, jsonResult.GetProperty("source").GetString());
                Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            }
            finally
            {
                // 确保资源清理
                CleanupFile(source);
                CleanupFile(destination);
            }
        }

        [Fact]
        public async Task MoveFileAsync_WithInvalidDestinationPath_ShouldReturnError()
        {
            // Arrange
            var source = GetUniqueFilePath("test.txt");
            var destination = "Z:\\invalid\\path\\dest.txt"; // 无效路径
            
            try
            {
                // 创建源文件
                File.WriteAllText(source, "Test content");
                
                var moveFileTool = new MoveFileTool(_fileSystemService, _mockLogger.Object);

                // Act
                var result = await moveFileTool.MoveFileAsync(source, destination);

                // Assert
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                Assert.False(jsonResult.GetProperty("success").GetBoolean());
                Assert.Equal(source, jsonResult.GetProperty("source").GetString());
                Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            }
            finally
            {
                // 确保资源清理
                CleanupFile(source);
            }
        }

        [Fact]
        public async Task MoveFileAsync_ToSameLocation_ShouldHandleGracefully()
        {
            // Arrange
            var filePath = GetUniqueFilePath("samefile.txt");
            
            try
            {
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
            }
            finally
            {
                // 确保资源清理
                CleanupFile(filePath);
            }
        }

        public void Dispose()
        {
            // 清理所有测试文件
            foreach (var file in _filesToCleanup)
            {
                CleanupFile(file);
            }
            
            // 清理测试目录
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch
            {
                // 忽略清理错误
            }
        }
    }
}
