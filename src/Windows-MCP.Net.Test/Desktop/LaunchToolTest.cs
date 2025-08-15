using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Tools.Desktop;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// LaunchTool单元测试类
    /// </summary>
    public class LaunchToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<LaunchTool>> _mockLogger;

        public LaunchToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<LaunchTool>>();
        }

        [Fact]
        public async Task LaunchAppAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var appName = "notepad";
            var expectedResult = "Application launched successfully";
            _mockDesktopService.Setup(x => x.LaunchAppAsync(appName))
                              .ReturnsAsync((expectedResult, 0));
            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await launchTool.LaunchAppAsync(appName);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.LaunchAppAsync(appName), Times.Once);
        }

        [Theory]
        [InlineData("calculator")]
        [InlineData("mspaint")]
        [InlineData("cmd")]
        public async Task LaunchAppAsync_WithDifferentApps_ShouldCallService(string appName)
        {
            // Arrange
            var expectedResult = $"Launched {appName}";
            _mockDesktopService.Setup(x => x.LaunchAppAsync(appName))
                              .ReturnsAsync((expectedResult, 0));
            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await launchTool.LaunchAppAsync(appName);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.LaunchAppAsync(appName), Times.Once);
        }

        [Fact]
        public async Task LaunchAppAsync_WithFailedLaunch_ShouldReturnErrorMessage()
        {
            // Arrange
            var appName = "nonexistentapp";
            var errorResponse = "App not found";
            var defaultLanguage = "English";
            _mockDesktopService.Setup(x => x.LaunchAppAsync(appName))
                              .ReturnsAsync((errorResponse, 1));
            _mockDesktopService.Setup(x => x.GetDefaultLanguage())
                              .Returns(defaultLanguage);
            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await launchTool.LaunchAppAsync(appName);

            // Assert
            var expectedMessage = $"Failed to launch {appName}. Try to use the app name in the default language ({defaultLanguage}).";
            Assert.Equal(expectedMessage, result);
            _mockDesktopService.Verify(x => x.LaunchAppAsync(appName), Times.Once);
            _mockDesktopService.Verify(x => x.GetDefaultLanguage(), Times.Once);
        }

        [Fact]
        public async Task LaunchAppAsync_WithSuccessfulLaunch_ShouldNotCallGetDefaultLanguage()
        {
            // Arrange
            var appName = "notepad";
            var successResponse = "Notepad launched";
            _mockDesktopService.Setup(x => x.LaunchAppAsync(appName))
                              .ReturnsAsync((successResponse, 0));
            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await launchTool.LaunchAppAsync(appName);

            // Assert
            Assert.Equal(successResponse, result);
            _mockDesktopService.Verify(x => x.LaunchAppAsync(appName), Times.Once);
            _mockDesktopService.Verify(x => x.GetDefaultLanguage(), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("app with spaces")]
        [InlineData("APP_WITH_UNDERSCORES")]
        public async Task LaunchAppAsync_WithVariousAppNames_ShouldCallService(string appName)
        {
            // Arrange
            var response = string.IsNullOrWhiteSpace(appName) ? "Invalid app name" : $"Launched {appName}";
            var statusCode = string.IsNullOrWhiteSpace(appName) ? 1 : 0;
            _mockDesktopService.Setup(x => x.LaunchAppAsync(appName))
                              .ReturnsAsync((response, statusCode));
            
            if (statusCode != 0)
            {
                _mockDesktopService.Setup(x => x.GetDefaultLanguage())
                                  .Returns("English");
            }

            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await launchTool.LaunchAppAsync(appName);

            // Assert
            if (statusCode == 0)
            {
                Assert.Equal(response, result);
            }
            else
            {
                var expectedMessage = $"Failed to launch {appName}. Try to use the app name in the default language (English).";
                Assert.Equal(expectedMessage, result);
            }
            _mockDesktopService.Verify(x => x.LaunchAppAsync(appName), Times.Once);
        }

        [Fact]
        public async Task LaunchAppAsync_WithNonZeroStatusCode_ShouldReturnFailureMessage()
        {
            // Arrange
            var appName = "failedapp";
            var errorResponse = "Launch failed";
            var statusCode = 2;
            var defaultLanguage = "中文";
            _mockDesktopService.Setup(x => x.LaunchAppAsync(appName))
                              .ReturnsAsync((errorResponse, statusCode));
            _mockDesktopService.Setup(x => x.GetDefaultLanguage())
                              .Returns(defaultLanguage);
            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await launchTool.LaunchAppAsync(appName);

            // Assert
            var expectedMessage = $"Failed to launch {appName}. Try to use the app name in the default language ({defaultLanguage}).";
            Assert.Equal(expectedMessage, result);
            _mockDesktopService.Verify(x => x.LaunchAppAsync(appName), Times.Once);
            _mockDesktopService.Verify(x => x.GetDefaultLanguage(), Times.Once);
        }

        [Fact]
        public async Task LaunchAppAsync_ConsecutiveLaunches_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var apps = new[]
            {
                ("notepad", "Notepad launched", 0),
                ("calculator", "Calculator launched", 0),
                ("invalidapp", "Not found", 1)
            };

            foreach (var (name, response, status) in apps)
            {
                _mockDesktopService.Setup(x => x.LaunchAppAsync(name))
                                  .ReturnsAsync((response, status));
            }

            _mockDesktopService.Setup(x => x.GetDefaultLanguage())
                              .Returns("English");

            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            foreach (var (appName, expectedResponse, statusCode) in apps)
            {
                var result = await launchTool.LaunchAppAsync(appName);
                
                if (statusCode == 0)
                {
                    Assert.Equal(expectedResponse, result);
                }
                else
                {
                    var expectedMessage = $"Failed to launch {appName}. Try to use the app name in the default language (English).";
                    Assert.Equal(expectedMessage, result);
                }
                
                _mockDesktopService.Verify(x => x.LaunchAppAsync(appName), Times.Once);
            }
        }

        [Fact]
        public async Task LaunchAppAsync_ServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var exception = new InvalidOperationException("Launch service error");
            _mockDesktopService.Setup(x => x.LaunchAppAsync(It.IsAny<string>()))
                              .ThrowsAsync(exception);
            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => launchTool.LaunchAppAsync("notepad"));
            Assert.Equal("Launch service error", thrownException.Message);
        }

        [Theory]
        [InlineData("chrome")]
        [InlineData("firefox")]
        [InlineData("edge")]
        public async Task LaunchAppAsync_WithBrowserApps_ShouldCallService(string browserName)
        {
            // Arrange
            var response = $"{browserName} browser launched";
            _mockDesktopService.Setup(x => x.LaunchAppAsync(browserName))
                              .ReturnsAsync((response, 0));
            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await launchTool.LaunchAppAsync(browserName);

            // Assert
            Assert.Equal(response, result);
            _mockDesktopService.Verify(x => x.LaunchAppAsync(browserName), Times.Once);
        }

        [Theory]
        [InlineData("powershell")]
        [InlineData("cmd")]
        [InlineData("wt")] // Windows Terminal
        public async Task LaunchAppAsync_WithTerminalApps_ShouldCallService(string terminalName)
        {
            // Arrange
            var response = $"{terminalName} terminal launched";
            _mockDesktopService.Setup(x => x.LaunchAppAsync(terminalName))
                              .ReturnsAsync((response, 0));
            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await launchTool.LaunchAppAsync(terminalName);

            // Assert
            Assert.Equal(response, result);
            _mockDesktopService.Verify(x => x.LaunchAppAsync(terminalName), Times.Once);
        }
    }
}
