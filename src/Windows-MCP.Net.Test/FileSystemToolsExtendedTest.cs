using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Tools.FileSystem;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test
{
    /// <summary>
    /// 扩展的FileSystem工具测试类
    /// </summary>
    public class FileSystemToolsExtendedTest
    {
        private readonly Mock<IFileSystemService> _mockFileSystemService;
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
            _mockFileSystemService = new Mock<IFileSystemService>();
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
            var source = "C:\\source.txt";
            var destination = "C:\\destination.txt";
            var expectedResult = "File copied successfully";
            _mockFileSystemService.Setup(x => x.CopyFileAsync(source, destination, false))
                                  .ReturnsAsync((expectedResult, 0));
            var copyFileTool = new CopyFileTool(_mockFileSystemService.Object, _mockCopyFileLogger.Object);

            // Act
            var result = await copyFileTool.CopyFileAsync(source, destination);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(source, jsonResult.GetProperty("source").GetString());
            Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            Assert.False(jsonResult.GetProperty("overwrite").GetBoolean());
            _mockFileSystemService.Verify(x => x.CopyFileAsync(source, destination, false), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CopyFileTool_CopyFileAsync_WithOverwrite_ShouldCallService(bool overwrite)
        {
            // Arrange
            var source = "C:\\test.txt";
            var destination = "C:\\copy.txt";
            var expectedResult = $"File copied with overwrite: {overwrite}";
            _mockFileSystemService.Setup(x => x.CopyFileAsync(source, destination, overwrite))
                                  .ReturnsAsync((expectedResult, 0));
            var copyFileTool = new CopyFileTool(_mockFileSystemService.Object, _mockCopyFileLogger.Object);

            // Act
            var result = await copyFileTool.CopyFileAsync(source, destination, overwrite);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(source, jsonResult.GetProperty("source").GetString());
            Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            Assert.Equal(overwrite, jsonResult.GetProperty("overwrite").GetBoolean());
            _mockFileSystemService.Verify(x => x.CopyFileAsync(source, destination, overwrite), Times.Once);
        }

        [Fact]
        public async Task MoveFileTool_MoveFileAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var source = "C:\\old.txt";
            var destination = "C:\\new.txt";
            var expectedResult = "File moved successfully";
            _mockFileSystemService.Setup(x => x.MoveFileAsync(source, destination, false))
                                  .ReturnsAsync((expectedResult, 0));
            var moveFileTool = new MoveFileTool(_mockFileSystemService.Object, _mockMoveFileLogger.Object);

            // Act
            var result = await moveFileTool.MoveFileAsync(source, destination);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(source, jsonResult.GetProperty("source").GetString());
            Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            _mockFileSystemService.Verify(x => x.MoveFileAsync(source, destination, false), Times.Once);
        }

        [Theory]
        [InlineData("C:\\Documents\\file1.txt", "D:\\Backup\\file1.txt")]
        [InlineData("C:\\temp\\data.json", "C:\\archive\\data.json")]
        public async Task MoveFileTool_MoveFileAsync_WithDifferentPaths_ShouldCallService(string source, string destination)
        {
            // Arrange
            var expectedResult = $"Moved from {source} to {destination}";
            _mockFileSystemService.Setup(x => x.MoveFileAsync(source, destination, false))
                                  .ReturnsAsync((expectedResult, 0));
            var moveFileTool = new MoveFileTool(_mockFileSystemService.Object, _mockMoveFileLogger.Object);

            // Act
            var result = await moveFileTool.MoveFileAsync(source, destination);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(source, jsonResult.GetProperty("source").GetString());
            Assert.Equal(destination, jsonResult.GetProperty("destination").GetString());
            _mockFileSystemService.Verify(x => x.MoveFileAsync(source, destination, false), Times.Once);
        }

        [Fact]
        public async Task DeleteFileTool_DeleteFileAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var filePath = "C:\\temp.txt";
            var expectedResult = "File deleted successfully";
            _mockFileSystemService.Setup(x => x.DeleteFileAsync(filePath))
                                  .ReturnsAsync((expectedResult, 0));
            var deleteFileTool = new DeleteFileTool(_mockFileSystemService.Object, _mockDeleteFileLogger.Object);

            // Act
            var result = await deleteFileTool.DeleteFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            _mockFileSystemService.Verify(x => x.DeleteFileAsync(filePath), Times.Once);
        }

        [Theory]
        [InlineData("C:\\test1.txt")]
        [InlineData("D:\\documents\\report.pdf")]
        [InlineData("C:\\temp\\image.jpg")]
        public async Task DeleteFileTool_DeleteFileAsync_WithDifferentFiles_ShouldCallService(string filePath)
        {
            // Arrange
            var expectedResult = $"Deleted {filePath}";
            _mockFileSystemService.Setup(x => x.DeleteFileAsync(filePath))
                                  .ReturnsAsync((expectedResult, 0));
            var deleteFileTool = new DeleteFileTool(_mockFileSystemService.Object, _mockDeleteFileLogger.Object);

            // Act
            var result = await deleteFileTool.DeleteFileAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            _mockFileSystemService.Verify(x => x.DeleteFileAsync(filePath), Times.Once);
        }

        [Fact]
        public async Task CreateFileTool_CreateFileAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var filePath = "C:\\newfile.txt";
            var content = "Hello World";
            var expectedResult = "File created successfully";
            _mockFileSystemService.Setup(x => x.CreateFileAsync(filePath, content))
                                  .ReturnsAsync((expectedResult, 0));
            var createFileTool = new CreateFileTool(_mockFileSystemService.Object, _mockCreateFileLogger.Object);

            // Act
            var result = await createFileTool.CreateFileAsync(filePath, content);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(content.Length, jsonResult.GetProperty("contentLength").GetInt32());
            _mockFileSystemService.Verify(x => x.CreateFileAsync(filePath, content), Times.Once);
        }

        [Theory]
        [InlineData("C:\\test.txt", "")]
        [InlineData("C:\\data.json", "{\"key\": \"value\"}")]
        [InlineData("C:\\readme.md", "# Title\n\nContent")]
        public async Task CreateFileTool_CreateFileAsync_WithDifferentContent_ShouldCallService(string filePath, string content)
        {
            // Arrange
            var expectedResult = $"Created {filePath} with content length: {content.Length}";
            _mockFileSystemService.Setup(x => x.CreateFileAsync(filePath, content))
                                  .ReturnsAsync((expectedResult, 0));
            var createFileTool = new CreateFileTool(_mockFileSystemService.Object, _mockCreateFileLogger.Object);

            // Act
            var result = await createFileTool.CreateFileAsync(filePath, content);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(content.Length, jsonResult.GetProperty("contentLength").GetInt32());
            _mockFileSystemService.Verify(x => x.CreateFileAsync(filePath, content), Times.Once);
        }

        [Fact]
        public async Task CreateDirectoryTool_CreateDirectoryAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var directoryPath = "C:\\NewFolder";
            var expectedResult = "Directory created successfully";
            _mockFileSystemService.Setup(x => x.CreateDirectoryAsync(directoryPath, true))
                                  .ReturnsAsync((expectedResult, 0));
            var createDirectoryTool = new CreateDirectoryTool(_mockFileSystemService.Object, _mockCreateDirectoryLogger.Object);

            // Act
            var result = await createDirectoryTool.CreateDirectoryAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.True(jsonResult.GetProperty("createParents").GetBoolean());
            _mockFileSystemService.Verify(x => x.CreateDirectoryAsync(directoryPath, true), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateDirectoryTool_CreateDirectoryAsync_WithCreateParents_ShouldCallService(bool createParents)
        {
            // Arrange
            var directoryPath = "C:\\Parent\\Child";
            var expectedResult = $"Directory created with createParents: {createParents}";
            _mockFileSystemService.Setup(x => x.CreateDirectoryAsync(directoryPath, createParents))
                                  .ReturnsAsync((expectedResult, 0));
            var createDirectoryTool = new CreateDirectoryTool(_mockFileSystemService.Object, _mockCreateDirectoryLogger.Object);

            // Act
            var result = await createDirectoryTool.CreateDirectoryAsync(directoryPath, createParents);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(createParents, jsonResult.GetProperty("createParents").GetBoolean());
            _mockFileSystemService.Verify(x => x.CreateDirectoryAsync(directoryPath, createParents), Times.Once);
        }

        [Fact]
        public async Task DeleteDirectoryTool_DeleteDirectoryAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var directoryPath = "C:\\TempFolder";
            var recursive = false;
            var expectedResult = "Directory deleted successfully";
            _mockFileSystemService.Setup(x => x.DeleteDirectoryAsync(directoryPath, recursive))
                                  .ReturnsAsync((expectedResult, 0));
            var deleteDirectoryTool = new DeleteDirectoryTool(_mockFileSystemService.Object, _mockDeleteDirectoryLogger.Object);

            // Act
            var result = await deleteDirectoryTool.DeleteDirectoryAsync(directoryPath, recursive);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.False(jsonResult.GetProperty("recursive").GetBoolean());
            _mockFileSystemService.Verify(x => x.DeleteDirectoryAsync(directoryPath, recursive), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DeleteDirectoryTool_DeleteDirectoryAsync_WithRecursive_ShouldCallService(bool recursive)
        {
            // Arrange
            var directoryPath = "C:\\TestFolder";
            var expectedResult = $"Directory deleted with recursive: {recursive}";
            _mockFileSystemService.Setup(x => x.DeleteDirectoryAsync(directoryPath, recursive))
                                  .ReturnsAsync((expectedResult, 0));
            var deleteDirectoryTool = new DeleteDirectoryTool(_mockFileSystemService.Object, _mockDeleteDirectoryLogger.Object);

            // Act
            var result = await deleteDirectoryTool.DeleteDirectoryAsync(directoryPath, recursive);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("message").GetString());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(recursive, jsonResult.GetProperty("recursive").GetBoolean());
            _mockFileSystemService.Verify(x => x.DeleteDirectoryAsync(directoryPath, recursive), Times.Once);
        }

        [Fact]
        public async Task ListDirectoryTool_ListDirectoryAsync_ShouldReturnDirectoryContents()
        {
            // Arrange
            var directoryPath = "C:\\TestDir";
            var expectedResult = "file1.txt\nfile2.txt\nsubfolder/";
            _mockFileSystemService.Setup(x => x.ListDirectoryAsync(directoryPath, true, true, false))
                                  .ReturnsAsync((expectedResult, 0));
            var listDirectoryTool = new ListDirectoryTool(_mockFileSystemService.Object, _mockListDirectoryLogger.Object);

            // Act
            var result = await listDirectoryTool.ListDirectoryAsync(directoryPath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("listing").GetString());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.True(jsonResult.GetProperty("includeFiles").GetBoolean());
            Assert.True(jsonResult.GetProperty("includeDirectories").GetBoolean());
            Assert.False(jsonResult.GetProperty("recursive").GetBoolean());
            _mockFileSystemService.Verify(x => x.ListDirectoryAsync(directoryPath, true, true, false), Times.Once);
        }

        [Theory]
        [InlineData(true, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        public async Task ListDirectoryTool_ListDirectoryAsync_WithOptions_ShouldCallService(bool includeFiles, bool includeDirectories, bool recursive)
        {
            // Arrange
            var directoryPath = "C:\\TestDir";
            var expectedResult = $"Listed with options: files={includeFiles}, dirs={includeDirectories}, recursive={recursive}";
            _mockFileSystemService.Setup(x => x.ListDirectoryAsync(directoryPath, includeFiles, includeDirectories, recursive))
                                  .ReturnsAsync((expectedResult, 0));
            var listDirectoryTool = new ListDirectoryTool(_mockFileSystemService.Object, _mockListDirectoryLogger.Object);

            // Act
            var result = await listDirectoryTool.ListDirectoryAsync(directoryPath, includeFiles, includeDirectories, recursive);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("listing").GetString());
            Assert.Equal(directoryPath, jsonResult.GetProperty("path").GetString());
            Assert.Equal(includeFiles, jsonResult.GetProperty("includeFiles").GetBoolean());
            Assert.Equal(includeDirectories, jsonResult.GetProperty("includeDirectories").GetBoolean());
            Assert.Equal(recursive, jsonResult.GetProperty("recursive").GetBoolean());
            _mockFileSystemService.Verify(x => x.ListDirectoryAsync(directoryPath, includeFiles, includeDirectories, recursive), Times.Once);
        }

        [Fact]
        public async Task GetFileInfoTool_GetFileInfoAsync_ShouldReturnFileInfo()
        {
            // Arrange
            var filePath = "C:\\test.txt";
            var expectedResult = "{\"name\": \"test.txt\", \"size\": 1024, \"created\": \"2024-01-01\"}";
            _mockFileSystemService.Setup(x => x.GetFileInfoAsync(filePath))
                                  .ReturnsAsync((expectedResult, 0));
            _mockFileSystemService.Setup(x => x.FileExists(filePath)).Returns(true);
            _mockFileSystemService.Setup(x => x.DirectoryExists(filePath)).Returns(false);
            var getFileInfoTool = new GetFileInfoTool(_mockFileSystemService.Object, _mockGetFileInfoLogger.Object);

            // Act
            var result = await getFileInfoTool.GetFileInfoAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("info").GetString());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.True(jsonResult.GetProperty("fileExists").GetBoolean());
            _mockFileSystemService.Verify(x => x.GetFileInfoAsync(filePath), Times.Once);
        }

        [Theory]
        [InlineData("C:\\document.pdf")]
        [InlineData("D:\\images\\photo.jpg")]
        [InlineData("C:\\data\\config.json")]
        public async Task GetFileInfoTool_GetFileInfoAsync_WithDifferentFiles_ShouldCallService(string filePath)
        {
            // Arrange
            var expectedResult = $"Info for {filePath}";
            _mockFileSystemService.Setup(x => x.GetFileInfoAsync(filePath))
                                  .ReturnsAsync((expectedResult, 0));
            _mockFileSystemService.Setup(x => x.FileExists(filePath)).Returns(true);
            _mockFileSystemService.Setup(x => x.DirectoryExists(filePath)).Returns(false);
            var getFileInfoTool = new GetFileInfoTool(_mockFileSystemService.Object, _mockGetFileInfoLogger.Object);

            // Act
            var result = await getFileInfoTool.GetFileInfoAsync(filePath);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.Equal(expectedResult, jsonResult.GetProperty("info").GetString());
            Assert.Equal(filePath, jsonResult.GetProperty("path").GetString());
            Assert.True(jsonResult.GetProperty("fileExists").GetBoolean());
            _mockFileSystemService.Verify(x => x.GetFileInfoAsync(filePath), Times.Once);
        }

        [Fact]
        public async Task SearchFilesTool_SearchFilesByNameAsync_ShouldReturnSearchResults()
        {
            // Arrange
            var directory = "C:\\Search";
            var pattern = "*.txt";
            var expectedResult = ("file1.txt\nfile2.txt\nsubdir\\file3.txt", 0);
            _mockFileSystemService.Setup(x => x.SearchFilesByNameAsync(directory, pattern, false))
                                  .ReturnsAsync(expectedResult);
            var searchTool = new SearchFilesTool(_mockFileSystemService.Object, _mockSearchFilesLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByNameAsync(directory, pattern);

            // Assert
            Assert.Contains("success", result);
            _mockFileSystemService.Verify(x => x.SearchFilesByNameAsync(directory, pattern, false), Times.Once);
        }

        [Theory]
        [InlineData("*.pdf", true)]
        [InlineData("test*", false)]
        [InlineData("*config*", true)]
        public async Task SearchFilesTool_SearchFilesByNameAsync_WithOptions_ShouldCallService(string pattern, bool recursive)
        {
            // Arrange
            var directory = "C:\\TestSearch";
            var expectedResult = $"Search results for {pattern} (recursive: {recursive})";
            _mockFileSystemService.Setup(x => x.SearchFilesByNameAsync(directory, pattern, recursive))
                                  .ReturnsAsync((expectedResult, 0));
            var searchTool = new SearchFilesTool(_mockFileSystemService.Object, _mockSearchFilesLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByNameAsync(directory, pattern, recursive);

            // Assert
            Assert.Contains("success", result);
            _mockFileSystemService.Verify(x => x.SearchFilesByNameAsync(directory, pattern, recursive), Times.Once);
        }

        [Fact]
        public async Task SearchFilesTool_SearchFilesByExtensionAsync_ShouldReturnSearchResults()
        {
            // Arrange
            var directory = "C:\\Files";
            var extension = ".txt";
            var expectedResult = ("document.txt\nnotes.txt\nreadme.txt", 0);
            _mockFileSystemService.Setup(x => x.SearchFilesByExtensionAsync(directory, extension, false))
                                  .ReturnsAsync(expectedResult);
            var searchTool = new SearchFilesTool(_mockFileSystemService.Object, _mockSearchFilesLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByExtensionAsync(directory, extension);

            // Assert
            Assert.Contains("success", result);
            _mockFileSystemService.Verify(x => x.SearchFilesByExtensionAsync(directory, extension, false), Times.Once);
        }

        [Theory]
        [InlineData(".pdf", true)]
        [InlineData("jpg", false)]
        [InlineData(".json", true)]
        public async Task SearchFilesTool_SearchFilesByExtensionAsync_WithOptions_ShouldCallService(string extension, bool recursive)
        {
            // Arrange
            var directory = "C:\\SearchTest";
            var expectedResult = $"Files with extension {extension} (recursive: {recursive})";
            _mockFileSystemService.Setup(x => x.SearchFilesByExtensionAsync(directory, extension, recursive))
                                  .ReturnsAsync((expectedResult, 0));
            var searchTool = new SearchFilesTool(_mockFileSystemService.Object, _mockSearchFilesLogger.Object);

            // Act
            var result = await searchTool.SearchFilesByExtensionAsync(directory, extension, recursive);

            // Assert
            Assert.Contains("success", result);
            _mockFileSystemService.Verify(x => x.SearchFilesByExtensionAsync(directory, extension, recursive), Times.Once);
        }
    }
}