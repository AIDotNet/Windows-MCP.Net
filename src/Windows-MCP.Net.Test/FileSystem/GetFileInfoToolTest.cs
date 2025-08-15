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
    /// GetFileInfoTool单元测试类
    /// </summary>
    public class GetFileInfoToolTest
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<GetFileInfoTool>> _mockLogger;

        public GetFileInfoToolTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockLogger = new Mock<ILogger<GetFileInfoTool>>();
        }

        [Fact]
        public async Task GetFileInfoAsync_ShouldReturnFileInfo()
        {
            // Arrange
            var filePath = "C:\\temp\\test.txt";
            var testContent = "This is test content for file info";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建测试文件
            File.WriteAllText(filePath, testContent);
            var getFileInfoTool = new GetFileInfoTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await getFileInfoTool.GetFileInfoAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.True(jsonResult.GetProperty("fileExists").GetBoolean());
            
            // 验证文件信息包含基本属性
            var info = jsonResult.GetProperty("info").GetString();
            Assert.Contains("test.txt", info);
            Assert.Contains(testContent.Length.ToString(), info);
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Theory]
        [InlineData("document.pdf")]
        [InlineData("photo.jpg")]
        [InlineData("config.json")]
        public async Task GetFileInfoAsync_WithDifferentFiles_ShouldCallService(string fileName)
        {
            // Arrange
            var filePath = $"C:\\temp\\{fileName}";
            var testContent = $"Test content for {fileName}";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建测试文件
            File.WriteAllText(filePath, testContent);
            var getFileInfoTool = new GetFileInfoTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await getFileInfoTool.GetFileInfoAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.True(jsonResult.GetProperty("fileExists").GetBoolean());
            
            // 验证文件信息包含文件名和大小
            var info = jsonResult.GetProperty("info").GetString();
            Assert.Contains(fileName, info);
            Assert.Contains(testContent.Length.ToString(), info);
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task GetFileInfoAsync_ForDirectory_ShouldReturnDirectoryInfo()
        {
            // Arrange
            var directoryPath = "C:\\temp\\testdir";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directoryPath);
            
            var getFileInfoTool = new GetFileInfoTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await getFileInfoTool.GetFileInfoAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.False(jsonResult.GetProperty("fileExists").GetBoolean());
            Assert.True(jsonResult.GetProperty("directoryExists").GetBoolean());
            
            // 验证目录信息
            var info = jsonResult.GetProperty("info").GetString();
            Assert.Contains("testdir", info);
            
            // 清理测试目录
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        [Fact]
        public async Task GetFileInfoAsync_WithNonExistentPath_ShouldReturnNotFound()
        {
            // Arrange
            var filePath = "C:\\temp\\nonexistent.txt";
            var getFileInfoTool = new GetFileInfoTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await getFileInfoTool.GetFileInfoAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.False(jsonResult.GetProperty("fileExists").GetBoolean());
            Assert.False(jsonResult.GetProperty("directoryExists").GetBoolean());
        }

        [Fact]
        public async Task GetFileInfoAsync_WithInvalidPath_ShouldReturnError()
        {
            // Arrange
            var filePath = "Z:\\invalid\\path\\file.txt"; // 无效路径
            var getFileInfoTool = new GetFileInfoTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await getFileInfoTool.GetFileInfoAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.False(jsonResult.GetProperty("fileExists").GetBoolean());
            Assert.False(jsonResult.GetProperty("directoryExists").GetBoolean());
        }

        [Fact]
        public async Task GetFileInfoAsync_WithEmptyFile_ShouldReturnInfo()
        {
            // Arrange
            var filePath = "C:\\temp\\empty.txt";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建空文件
            File.WriteAllText(filePath, "");
            var getFileInfoTool = new GetFileInfoTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await getFileInfoTool.GetFileInfoAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.True(jsonResult.GetProperty("fileExists").GetBoolean());
            
            // 验证文件信息显示大小为0
            var info = jsonResult.GetProperty("info").GetString();
            Assert.Contains("empty.txt", info);
            Assert.Contains("0", info);
            
            // 清理测试文件
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
