using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Tools.FileSystem;
using WindowsMCP.Net.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Windows_MCP.Net.Test
{
    /// <summary>
    /// 扩展的FileSystem工具测试类
    /// </summary>
    public class FileSystemToolsExtendedTest
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly Mock<ILogger<CopyFileTool>> _mockCopyFileLogger;
        private readonly Mock<ILogger<MoveFileTool>> _mockMoveFileLogger;
        private readonly Mock<ILogger<DeleteFileTool>> _mockDeleteFileLogger;
        private readonly Mock<ILogger<CreateFileTool>> _mockCreateFileLogger;
        private readonly Mock<ILogger<CreateDirectoryTool>> _mockCreateDirectoryLogger;
        private readonly Mock<ILogger<DeleteDirectoryTool>> _mockDeleteDirectoryLogger;
        private readonly Mock<ILogger<ListDirectoryTool>> _mockListDirectoryLogger;
        private readonly Mock<ILogger<GetFileInfoTool>> _mockGetFileInfoLogger;
        private readonly Mock<ILogger<SearchFilesTool>> _mockSearchFilesLogger;

        public FileSystemToolsExtendedTest()
        {
            _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
            _mockCopyFileLogger = new Mock<ILogger<CopyFileTool>>();
            _mockMoveFileLogger = new Mock<ILogger<MoveFileTool>>();
            _mockDeleteFileLogger = new Mock<ILogger<DeleteFileTool>>();
            _mockCreateFileLogger = new Mock<ILogger<CreateFileTool>>();
            _mockCreateDirectoryLogger = new Mock<ILogger<CreateDirectoryTool>>();
            _mockDeleteDirectoryLogger = new Mock<ILogger<DeleteDirectoryTool>>();
            _mockListDirectoryLogger = new Mock<ILogger<ListDirectoryTool>>();
            _mockGetFileInfoLogger = new Mock<ILogger<GetFileInfoTool>>();
            _mockSearchFilesLogger = new Mock<ILogger<SearchFilesTool>>();
        }

        [Fact]
        public async Task CopyFileTool_CopyFileAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var source = "C:\\temp\\source.txt";
            var destination = "C:\\temp\\destination.txt";
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            // 创建源文件用于测试
            if (!File.Exists(source))
            {
                File.WriteAllText(source, "Test content");
            }
            // 确保目标文件不存在
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }
            
            var copyFileTool = new CopyFileTool(_fileSystemService, _mockCopyFileLogger.Object);

            // Act
            var result = await copyFileTool.CopyFileAsync(source, destination);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(source, jsonResult.GetProperty("source").GetString());
            Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            Assert.False(jsonResult.GetProperty("overwrite").GetBoolean());
            // 验证文件是否确实被复制
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
        public async Task CopyFileTool_CopyFileAsync_WithOverwrite_ShouldCallService(bool overwrite)
        {
            // Arrange
            var source = "C:\\temp\\test.txt";
            var destination = "C:\\temp\\copy.txt";
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            // 创建源文件
            File.WriteAllText(source, "Test content for overwrite test");
            
            if (overwrite)
            {
                // 如果测试覆盖，先创建目标文件
                File.WriteAllText(destination, "Original content");
            }
            else
            {
                // 确保目标文件不存在
                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }
            }
            
            var copyFileTool = new CopyFileTool(_fileSystemService, _mockCopyFileLogger.Object);

            // Act
            var result = await copyFileTool.CopyFileAsync(source, destination, overwrite);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(source, jsonResult.GetProperty("source").GetString());
            Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            Assert.Equal(overwrite, jsonResult.GetProperty("overwrite").GetBoolean());
            // 验证文件是否确实被复制
            Assert.True(File.Exists(destination));
            
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
        public async Task MoveFileTool_MoveFileAsync_ShouldReturnSuccessMessage()
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
            
            var moveFileTool = new MoveFileTool(_fileSystemService, _mockMoveFileLogger.Object);

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
        public async Task MoveFileTool_MoveFileAsync_WithDifferentPaths_ShouldCallService(string sourceFile, string destFile)
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
            
            var moveFileTool = new MoveFileTool(_fileSystemService, _mockMoveFileLogger.Object);

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

        [Fact]
        public async Task DeleteFileTool_DeleteFileAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var filePath = "C:\\temp\\temp.txt";
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建要删除的文件
            File.WriteAllText(filePath, "This file will be deleted");
            
            var deleteFileTool = new DeleteFileTool(_fileSystemService, _mockDeleteFileLogger.Object);

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
        public async Task DeleteFileTool_DeleteFileAsync_WithDifferentFiles_ShouldCallService(string fileName)
        {
            // Arrange
            var filePath = Path.Combine("C:\\temp", fileName);
            
            // 确保测试目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建要删除的文件
            File.WriteAllText(filePath, $"Content for {fileName}");
            
            var deleteFileTool = new DeleteFileTool(_fileSystemService, _mockDeleteFileLogger.Object);

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
        public async Task CreateFileTool_CreateFileAsync_ShouldReturnSuccessMessage()
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
            
            var createFileTool = new CreateFileTool(_fileSystemService, _mockCreateFileLogger.Object);

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
        public async Task CreateFileTool_CreateFileAsync_WithDifferentContent_ShouldCallService(string fileName, string content)
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
            
            var createFileTool = new CreateFileTool(_fileSystemService, _mockCreateFileLogger.Object);

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
        public async Task CreateDirectoryTool_CreateDirectoryAsync_ShouldReturnSuccessMessage()
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
            
            var createDirectoryTool = new CreateDirectoryTool(_fileSystemService, _mockCreateDirectoryLogger.Object);

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
        public async Task CreateDirectoryTool_CreateDirectoryAsync_WithCreateParents_ShouldCallService(bool createParents)
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
            
            var createDirectoryTool = new CreateDirectoryTool(_fileSystemService, _mockCreateDirectoryLogger.Object);

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
        public async Task DeleteDirectoryTool_DeleteDirectoryAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var directoryPath = "C:\\temp\\TempFolder";
            var recursive = false;
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建要删除的目录
            Directory.CreateDirectory(directoryPath);
            
            var deleteDirectoryTool = new DeleteDirectoryTool(_fileSystemService, _mockDeleteDirectoryLogger.Object);

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
        public async Task DeleteDirectoryTool_DeleteDirectoryAsync_WithRecursive_ShouldCallService(bool recursive)
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
            
            var deleteDirectoryTool = new DeleteDirectoryTool(_fileSystemService, _mockDeleteDirectoryLogger.Object);

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
        public async Task ListDirectoryTool_ListDirectoryAsync_ShouldReturnDirectoryContents()
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
            
            var listDirectoryTool = new ListDirectoryTool(_fileSystemService, _mockListDirectoryLogger.Object);

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
        public async Task ListDirectoryTool_ListDirectoryAsync_WithOptions_ShouldCallService(bool includeFiles, bool includeDirectories, bool recursive)
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
            
            var listDirectoryTool = new ListDirectoryTool(_fileSystemService, _mockListDirectoryLogger.Object);

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
        public async Task GetFileInfoTool_GetFileInfoAsync_ShouldReturnFileInfo()
        {
            // Arrange
            var filePath = "C:\\temp\\test.txt";
            var testContent = "This is test content for file info";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建测试文件
            File.WriteAllText(filePath, testContent);
            var getFileInfoTool = new GetFileInfoTool(_fileSystemService, _mockGetFileInfoLogger.Object);

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
        public async Task GetFileInfoTool_GetFileInfoAsync_WithDifferentFiles_ShouldCallService(string fileName)
        {
            // Arrange
            var filePath = $"C:\\temp\\{fileName}";
            var testContent = $"Test content for {fileName}";
            
            // 确保基础目录存在
            Directory.CreateDirectory("C:\\temp");
            
            // 创建测试文件
            File.WriteAllText(filePath, testContent);
            var getFileInfoTool = new GetFileInfoTool(_fileSystemService, _mockGetFileInfoLogger.Object);

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
        public async Task SearchFilesTool_SearchFilesByNameAsync_ShouldReturnSearchResults()
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
            
            var searchTool = new SearchFilesTool(_fileSystemService, _mockSearchFilesLogger.Object);

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
        public async Task SearchFilesTool_SearchFilesByNameAsync_WithOptions_ShouldCallService(string pattern, bool recursive)
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
            
            var searchTool = new SearchFilesTool(_fileSystemService, _mockSearchFilesLogger.Object);

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
        public async Task SearchFilesTool_SearchFilesByExtensionAsync_ShouldReturnSearchResults()
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
            
            var searchTool = new SearchFilesTool(_fileSystemService, _mockSearchFilesLogger.Object);

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
        public async Task SearchFilesTool_SearchFilesByExtensionAsync_WithOptions_ShouldCallService(string extension, bool recursive)
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
            
            var searchTool = new SearchFilesTool(_fileSystemService, _mockSearchFilesLogger.Object);

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
    }
}