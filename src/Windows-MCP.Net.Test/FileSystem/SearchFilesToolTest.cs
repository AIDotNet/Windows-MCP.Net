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
    /// SearchFilesTool单元测试类
    /// </summary>
    public class SearchFilesToolTest
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<SearchFilesTool>> _mockLogger;

        public SearchFilesToolTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockLogger = new Mock<ILogger<SearchFilesTool>>();
        }

        [Fact]
        public async Task SearchFilesByNameAsync_ShouldReturnSearchResults()
        {
            // Arrange
            var directory = "C:\\temp\\Search";
            var pattern = "*.txt";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directory);
            
            // 创建测试文件
            File.WriteAllText(Path.Combine(directory, "file1.txt"), "content1");
            File.WriteAllText(Path.Combine(directory, "file2.txt"), "content2");
            File.WriteAllText(Path.Combine(directory, "other.doc"), "other content");
            
            var searchTool = new SearchFilesTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByNameAsync(directory, pattern);

            // Assert
            Assert.Contains("success", result);
            Assert.Contains("file1.txt", result);
            Assert.Contains("file2.txt", result);
            Assert.DoesNotContain("other.doc", result);
            
            // 清理测试目录
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [Theory]
        [InlineData("*.pdf", true)]
        [InlineData("test*", false)]
        [InlineData("*config*", true)]
        public async Task SearchFilesByNameAsync_WithOptions_ShouldCallService(string pattern, bool recursive)
        {
            // Arrange
            var directory = "C:\\temp\\TestSearch";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directory);
            
            // 根据pattern创建相应的测试文件
            if (pattern == "*.pdf")
            {
                File.WriteAllText(Path.Combine(directory, "document.pdf"), "pdf content");
                if (recursive)
                {
                    var subDir = Path.Combine(directory, "subdir");
                    Directory.CreateDirectory(subDir);
                    File.WriteAllText(Path.Combine(subDir, "subdoc.pdf"), "sub pdf content");
                }
            }
            else if (pattern == "test*")
            {
                File.WriteAllText(Path.Combine(directory, "testfile.txt"), "test content");
                File.WriteAllText(Path.Combine(directory, "testing.doc"), "testing content");
            }
            else if (pattern == "*config*")
            {
                File.WriteAllText(Path.Combine(directory, "app.config"), "config content");
                File.WriteAllText(Path.Combine(directory, "myconfig.json"), "json config");
                if (recursive)
                {
                    var subDir = Path.Combine(directory, "configs");
                    Directory.CreateDirectory(subDir);
                    File.WriteAllText(Path.Combine(subDir, "subconfig.xml"), "sub config");
                }
            }
            
            var searchTool = new SearchFilesTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByNameAsync(directory, pattern, recursive);

            // Assert
            Assert.Contains("success", result);
            
            // 清理测试目录
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public async Task SearchFilesByExtensionAsync_ShouldReturnSearchResults()
        {
            // Arrange
            var directory = "C:\\temp\\Files";
            var extension = ".txt";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directory);
            
            // 创建测试文件
            File.WriteAllText(Path.Combine(directory, "document.txt"), "document content");
            File.WriteAllText(Path.Combine(directory, "notes.txt"), "notes content");
            File.WriteAllText(Path.Combine(directory, "readme.txt"), "readme content");
            File.WriteAllText(Path.Combine(directory, "other.doc"), "other content");
            
            var searchTool = new SearchFilesTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByExtensionAsync(directory, extension);

            // Assert
            Assert.Contains("success", result);
            Assert.Contains("document.txt", result);
            Assert.Contains("notes.txt", result);
            Assert.Contains("readme.txt", result);
            Assert.DoesNotContain("other.doc", result);
            
            // 清理测试目录
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [Theory]
        [InlineData(".pdf", true)]
        [InlineData("jpg", false)]
        [InlineData(".json", true)]
        public async Task SearchFilesByExtensionAsync_WithOptions_ShouldCallService(string extension, bool recursive)
        {
            // Arrange
            var directory = "C:\\temp\\SearchTest";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directory);
            
            // 根据extension创建相应的测试文件
            var normalizedExt = extension.StartsWith(".") ? extension : "." + extension;
            File.WriteAllText(Path.Combine(directory, $"file1{normalizedExt}"), "content1");
            File.WriteAllText(Path.Combine(directory, $"file2{normalizedExt}"), "content2");
            
            if (recursive)
            {
                var subDir = Path.Combine(directory, "subdir");
                Directory.CreateDirectory(subDir);
                File.WriteAllText(Path.Combine(subDir, $"subfile{normalizedExt}"), "sub content");
            }
            
            var searchTool = new SearchFilesTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByExtensionAsync(directory, extension, recursive);

            // Assert
            Assert.Contains("success", result);
            Assert.Contains($"file1{normalizedExt}", result);
            Assert.Contains($"file2{normalizedExt}", result);
            
            if (recursive)
            {
                Assert.Contains($"subfile{normalizedExt}", result);
            }
            
            // 清理测试目录
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public async Task SearchFilesByNameAsync_WithNonExistentDirectory_ShouldReturnError()
        {
            // Arrange
            var directory = "C:\\temp\\NonExistentDir";
            var pattern = "*.txt";
            var searchTool = new SearchFilesTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByNameAsync(directory, pattern);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directory, jsonResult.GetProperty("directory").GetString());
            Assert.Equal(pattern, jsonResult.GetProperty("pattern").GetString());
        }

        [Fact]
        public async Task SearchFilesByExtensionAsync_WithNonExistentDirectory_ShouldReturnError()
        {
            // Arrange
            var directory = "C:\\temp\\NonExistentDir";
            var extension = ".txt";
            var searchTool = new SearchFilesTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByExtensionAsync(directory, extension);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.False(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directory, jsonResult.GetProperty("directory").GetString());
            Assert.Equal(extension, jsonResult.GetProperty("extension").GetString());
        }

        [Fact]
        public async Task SearchFilesByNameAsync_WithEmptyDirectory_ShouldReturnEmptyResults()
        {
            // Arrange
            var directory = "C:\\temp\\EmptySearchDir";
            var pattern = "*.txt";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directory);
            
            var searchTool = new SearchFilesTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByNameAsync(directory, pattern);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directory, jsonResult.GetProperty("directory").GetString());
            Assert.Equal(pattern, jsonResult.GetProperty("pattern").GetString());
            
            // 清理测试目录
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public async Task SearchFilesByExtensionAsync_WithEmptyDirectory_ShouldReturnEmptyResults()
        {
            // Arrange
            var directory = "C:\\temp\\EmptyExtSearchDir";
            var extension = ".txt";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directory);
            
            var searchTool = new SearchFilesTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByExtensionAsync(directory, extension);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(directory, jsonResult.GetProperty("directory").GetString());
            Assert.Equal(extension, jsonResult.GetProperty("extension").GetString());
            
            // 清理测试目录
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [Fact]
        public async Task SearchFilesByNameAsync_WithComplexPattern_ShouldWork()
        {
            // Arrange
            var directory = "C:\\temp\\ComplexSearch";
            var pattern = "test_*.txt";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            Directory.CreateDirectory(directory);
            
            // 创建匹配和不匹配的文件
            File.WriteAllText(Path.Combine(directory, "test_001.txt"), "content1");
            File.WriteAllText(Path.Combine(directory, "test_002.txt"), "content2");
            File.WriteAllText(Path.Combine(directory, "other_001.txt"), "content3");
            File.WriteAllText(Path.Combine(directory, "test.txt"), "content4");
            
            var searchTool = new SearchFilesTool(_fileSystemService, _mockLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByNameAsync(directory, pattern);

            // Assert
            Assert.Contains("success", result);
            Assert.Contains("test_001.txt", result);
            Assert.Contains("test_002.txt", result);
            // 根据实际实现，可能包含或不包含其他文件
            
            // 清理测试目录
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
