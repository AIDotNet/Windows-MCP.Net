using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// PowershellTool单元测试类
    /// </summary>
    public class PowershellToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<PowershellTool>> _mockLogger;

        public PowershellToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<PowershellTool>>();
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldReturnCommandOutput()
        {
            // Arrange
            var command = "Get-Date";
            var expectedResult = "2024-01-01 12:00:00";
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(command))
                              .ReturnsAsync((expectedResult, 0));
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.Equal($"Status Code: 0\nResponse: {expectedResult}", result);
            _mockDesktopService.Verify(x => x.ExecuteCommandAsync(command), Times.Once);
        }

        [Theory]
        [InlineData("Get-Process")]
        [InlineData("Get-Location")]
        [InlineData("ls")]
        public async Task ExecuteCommandAsync_WithDifferentCommands_ShouldCallService(string command)
        {
            // Arrange
            var expectedResult = $"Output of {command}";
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(command))
                              .ReturnsAsync((expectedResult, 0));
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await powershellTool.ExecuteCommandAsync(command);

            // Assert
            var expectedOutput = $"Status Code: 0\nResponse: {expectedResult}";
            Assert.Equal(expectedOutput, result);
            _mockDesktopService.Verify(x => x.ExecuteCommandAsync(command), Times.Once);
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithErrorCommand_ShouldReturnErrorStatus()
        {
            // Arrange
            var command = "Get-NonExistentCommand";
            var errorMessage = "Command not found";
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(command))
                              .ReturnsAsync((errorMessage, 1));
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.Equal($"Status Code: 1\nResponse: {errorMessage}", result);
            _mockDesktopService.Verify(x => x.ExecuteCommandAsync(command), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Get-ChildItem")]
        [InlineData("Get-Process | Select-Object Name")]
        public async Task ExecuteCommandAsync_WithVariousCommands_ShouldCallService(string command)
        {
            // Arrange
            var response = $"Executed: {command}";
            var statusCode = string.IsNullOrWhiteSpace(command) ? 1 : 0;
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(command))
                              .ReturnsAsync((response, statusCode));
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.Equal($"Status Code: {statusCode}\nResponse: {response}", result);
            _mockDesktopService.Verify(x => x.ExecuteCommandAsync(command), Times.Once);
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithComplexCommand_ShouldCallService()
        {
            // Arrange
            var command = "Get-Process | Where-Object {$_.ProcessName -eq 'notepad'} | Select-Object Id, ProcessName";
            var response = "Id ProcessName\n-- -----------\n1234 notepad";
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(command))
                              .ReturnsAsync((response, 0));
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.Equal($"Status Code: 0\nResponse: {response}", result);
            _mockDesktopService.Verify(x => x.ExecuteCommandAsync(command), Times.Once);
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithMultilineOutput_ShouldReturnFormattedResult()
        {
            // Arrange
            var command = "Get-Service";
            var response = "Status   Name               DisplayName\n------   ----               -----------\nRunning  AudioSrv           Windows Audio\nStopped  BITS               Background Intelligent Transfer Ser...";
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(command))
                              .ReturnsAsync((response, 0));
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.Equal($"Status Code: 0\nResponse: {response}", result);
            Assert.Contains("Status Code: 0", result);
            Assert.Contains("Response: ", result);
            _mockDesktopService.Verify(x => x.ExecuteCommandAsync(command), Times.Once);
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithEmptyOutput_ShouldReturnFormattedResult()
        {
            // Arrange
            var command = "Clear-Host";
            var response = "";
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(command))
                              .ReturnsAsync((response, 0));
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.Equal($"Status Code: 0\nResponse: {response}", result);
            _mockDesktopService.Verify(x => x.ExecuteCommandAsync(command), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(255)]
        public async Task ExecuteCommandAsync_WithDifferentStatusCodes_ShouldReturnCorrectFormat(int statusCode)
        {
            // Arrange
            var command = "Test-Command";
            var response = $"Command result with status {statusCode}";
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(command))
                              .ReturnsAsync((response, statusCode));
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.Equal($"Status Code: {statusCode}\nResponse: {response}", result);
            _mockDesktopService.Verify(x => x.ExecuteCommandAsync(command), Times.Once);
        }

        [Fact]
        public async Task ExecuteCommandAsync_ServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var exception = new InvalidOperationException("PowerShell service error");
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(It.IsAny<string>()))
                              .ThrowsAsync(exception);
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => powershellTool.ExecuteCommandAsync("Get-Date"));
            Assert.Equal("PowerShell service error", thrownException.Message);
        }

        [Fact]
        public async Task ExecuteCommandAsync_ConsecutiveCommands_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var commands = new[]
            {
                ("Get-Date", "2024-01-01", 0),
                ("Get-Location", "C:\\", 0),
                ("Get-Process", "Process list", 0)
            };

            foreach (var (cmd, resp, status) in commands)
            {
                _mockDesktopService.Setup(x => x.ExecuteCommandAsync(cmd))
                                  .ReturnsAsync((resp, status));
            }

            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            foreach (var (command, response, statusCode) in commands)
            {
                var result = await powershellTool.ExecuteCommandAsync(command);
                Assert.Equal($"Status Code: {statusCode}\nResponse: {response}", result);
                _mockDesktopService.Verify(x => x.ExecuteCommandAsync(command), Times.Once);
            }
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithSpecialCharacters_ShouldCallService()
        {
            // Arrange
            var command = "Write-Output 'Hello, 世界! @#$%^&*()'";
            var response = "Hello, 世界! @#$%^&*()";
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(command))
                              .ReturnsAsync((response, 0));
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.Equal($"Status Code: 0\nResponse: {response}", result);
            _mockDesktopService.Verify(x => x.ExecuteCommandAsync(command), Times.Once);
        }
    }
}
