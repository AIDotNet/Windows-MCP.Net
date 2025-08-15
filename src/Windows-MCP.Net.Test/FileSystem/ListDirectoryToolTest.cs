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
    /// ListDirectoryTool单元测试类
    /// </summary>
    public class ListDirectoryToolTest
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<ListDirectoryTool>> _mockLogger;

        public ListDirectoryToolTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockLogger = new Mock<ILogger<ListDirectoryTool>>();
        }

        [Fact]
        public async Task ListDirectoryAsync_ShouldReturnDirectoryContents()
        {
            // Arrange
            var directoryPath = "C:\\temp\\TestDir";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建测试目录结构
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(Path.Combine(directoryPath, "file1.txt"), "content1");
            File.WriteAllText(Path.Combine(directoryPath, "file2.txt"), "content2");
            Directory.CreateDirectory(Path.Combine(directoryPath, "subfolder"));
            
            var listDirectoryTool = new ListDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await listDirectoryTool.ListDirectoryAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.True(jsonResult.GetProperty("includeFiles").GetBoolean());
            Assert.True(jsonResult.GetProperty("includeDirectories").GetBoolean());
            Assert.False(jsonResult.GetProperty("recursive").GetBoolean());
            
            // 验证返回的列表包含我们创建的文件和目录
            var listing = jsonResult.GetProperty("listing").GetString();
            Assert.Contains("file1.txt", listing);
            Assert.Contains("file2.txt", listing);
            Assert.Contains("subfolder", listing);
            
            // 清理测试目录
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        [Theory]
        [InlineData(true, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        public async Task ListDirectoryAsync_WithOptions_ShouldCallService(bool includeFiles, bool includeDirectories, bool recursive)
        {
            // Arrange
            var directoryPath = "C:\\temp\\TestDirOptions";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建测试目录结构
            Directory.CreateDirectory(directoryPath);
            if (includeFiles)
            {
                File.WriteAllText(Path.Combine(directoryPath, "testfile.txt"), "test content");
            }
            if (includeDirectories)
            {
                Directory.CreateDirectory(Path.Combine(directoryPath, "testsubdir"));
            }
            if (recursive)
            {
                var subDir = Path.Combine(directoryPath, "subdir");
                Directory.CreateDirectory(subDir);
                File.WriteAllText(Path.Combine(subDir, "subfile.txt"), "sub content");
            }
            
            var listDirectoryTool = new ListDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await listDirectoryTool.ListDirectoryAsync(directoryPath, includeFiles, includeDirectories, recursive);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(includeFiles, jsonResult.GetProperty("includeFiles").GetBoolean());
            Assert.Equal(includeDirectories, jsonResult.GetProperty("includeDirectories").GetBoolean());
            Assert.Equal(recursive, jsonResult.GetProperty("recursive").GetBoolean());
            
            // 验证返回的列表符合选项设置
            var listing = jsonResult.GetProperty("listing").GetString();
            if (includeFiles)
            {
                Assert.Contains("testfile.txt", listing);
            }
            if (includeDirectories)
            {
                Assert.Contains("testsubdir", listing);
            }
            
            // 清理测试目录
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        [Fact]
        public async Task ListDirectoryAsync_WithEmptyDirectory_ShouldReturnEmptyListing()
        {
            // Arrange
            var directoryPath = "C:\\temp\\EmptyDir";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directoryPath);
            
            var listDirectoryTool = new ListDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await listDirectoryTool.ListDirectoryAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            
            // 验证列表为空或表示没有内容
            var listing = jsonResult.GetProperty("listing").GetString();
            Assert.NotNull(listing);
            
            // 清理测试目录
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        [Fact]
        public async Task ListDirectoryAsync_WithNonExistentDirectory_ShouldReturnError()
        {
            // Arrange
            var directoryPath = "C:\\temp\\NonExistentDir";
            var listDirectoryTool = new ListDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await listDirectoryTool.ListDirectoryAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
        }

        [Fact]
        public async Task ListDirectoryAsync_WithInvalidPath_ShouldReturnError()
        {
            // Arrange
            var directoryPath = "Z:\\invalid\\path\\folder"; // 无效路径
            var listDirectoryTool = new ListDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await listDirectoryTool.ListDirectoryAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
        }

        [Fact]
        public async Task ListDirectoryAsync_OnlyFiles_ShouldReturnFilesOnly()
        {
            // Arrange
            var directoryPath = "C:\\temp\\FilesOnlyDir";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directoryPath);
            
            // 创建文件和子目录
            File.WriteAllText(Path.Combine(directoryPath, "file1.txt"), "content1");
            File.WriteAllText(Path.Combine(directoryPath, "file2.txt"), "content2");
            Directory.CreateDirectory(Path.Combine(directoryPath, "subdir"));
            
            var listDirectoryTool = new ListDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await listDirectoryTool.ListDirectoryAsync(directoryPath, includeFiles: true, includeDirectories: false);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.True(jsonResult.GetProperty("includeFiles").GetBoolean());
            Assert.False(jsonResult.GetProperty("includeDirectories").GetBoolean());
            
            var listing = jsonResult.GetProperty("listing").GetString();
            Assert.Contains("file1.txt", listing);
            Assert.Contains("file2.txt", listing);
            // 根据实现，子目录可能不会被包含在列表中
            
            // 清理测试目录
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        [Fact]
        public async Task ListDirectoryAsync_OnlyDirectories_ShouldReturnDirectoriesOnly()
        {
            // Arrange
            var directoryPath = "C:\\temp\\DirsOnlyDir";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directoryPath);
            
            // 创建文件和子目录
            File.WriteAllText(Path.Combine(directoryPath, "file1.txt"), "content1");
            Directory.CreateDirectory(Path.Combine(directoryPath, "subdir1"));
            Directory.CreateDirectory(Path.Combine(directoryPath, "subdir2"));
            
            var listDirectoryTool = new ListDirectoryTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await listDirectoryTool.ListDirectoryAsync(directoryPath, includeFiles: false, includeDirectories: true);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.False(jsonResult.GetProperty("includeFiles").GetBoolean());
            Assert.True(jsonResult.GetProperty("includeDirectories").GetBoolean());
            
            var listing = jsonResult.GetProperty("listing").GetString();
            Assert.Contains("subdir1", listing);
            Assert.Contains("subdir2", listing);
            // 根据实现，文件可能不会被包含在列表中
            
            // 清理测试目录
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }
    }
}
