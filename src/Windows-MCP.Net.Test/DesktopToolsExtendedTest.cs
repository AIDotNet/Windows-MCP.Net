using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Tools.Desktop;

namespace Windows_MCP.Net.Test
{
    /// <summary>
    /// 扩展的Desktop工具测试类
    /// </summary>
    public class DesktopToolsExtendedTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<MoveTool>> _mockMoveLogger;
        private readonly Mock<ILogger<DragTool>> _mockDragLogger;
        private readonly Mock<ILogger<ScrollTool>> _mockScrollLogger;
        private readonly Mock<ILogger<KeyTool>> _mockKeyLogger;
        private readonly Mock<ILogger<ShortcutTool>> _mockShortcutLogger;
        private readonly Mock<ILogger<WaitTool>> _mockWaitLogger;
        private readonly Mock<ILogger<ScreenshotTool>> _mockScreenshotLogger;
        private readonly Mock<ILogger<LaunchTool>> _mockLaunchLogger;
        private readonly Mock<ILogger<SwitchTool>> _mockSwitchLogger;
        private readonly Mock<ILogger<ResizeTool>> _mockResizeLogger;
        private readonly Mock<ILogger<GetWindowInfoTool>> _mockWindowInfoLogger;
        private readonly Mock<ILogger<StateTool>> _mockStateLogger;
        private readonly Mock<ILogger<PowershellTool>> _mockPowershellLogger;
        private readonly Mock<ILogger<ScrapeTool>> _mockScrapeLogger;
        private readonly Mock<ILogger<OpenBrowserTool>> _mockBrowserLogger;

        public DesktopToolsExtendedTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockMoveLogger = new Mock<ILogger<MoveTool>>();
            _mockDragLogger = new Mock<ILogger<DragTool>>();
            _mockScrollLogger = new Mock<ILogger<ScrollTool>>();
            _mockKeyLogger = new Mock<ILogger<KeyTool>>();
            _mockShortcutLogger = new Mock<ILogger<ShortcutTool>>();
            _mockWaitLogger = new Mock<ILogger<WaitTool>>();
            _mockScreenshotLogger = new Mock<ILogger<ScreenshotTool>>();
            _mockLaunchLogger = new Mock<ILogger<LaunchTool>>();
            _mockSwitchLogger = new Mock<ILogger<SwitchTool>>();
            _mockResizeLogger = new Mock<ILogger<ResizeTool>>();
            _mockWindowInfoLogger = new Mock<ILogger<GetWindowInfoTool>>();
            _mockStateLogger = new Mock<ILogger<StateTool>>();
            _mockPowershellLogger = new Mock<ILogger<PowershellTool>>();
            _mockScrapeLogger = new Mock<ILogger<ScrapeTool>>();
            _mockBrowserLogger = new Mock<ILogger<OpenBrowserTool>>();
        }

        [Fact]
        public async Task MoveTool_MoveAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Mouse moved successfully";
            _mockDesktopService.Setup(x => x.MoveAsync(100, 200))
                              .ReturnsAsync(expectedResult);
            var moveTool = new MoveTool(_mockDesktopService.Object, _mockMoveLogger.Object);

            // Act
            var result = await moveTool.MoveAsync(100, 200);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.MoveAsync(100, 200), Times.Once);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1920, 1080)]
        [InlineData(500, 300)]
        public async Task MoveTool_MoveAsync_WithDifferentCoordinates_ShouldCallService(int x, int y)
        {
            // Arrange
            var expectedResult = $"Moved to ({x}, {y})";
            _mockDesktopService.Setup(s => s.MoveAsync(x, y))
                              .ReturnsAsync(expectedResult);
            var moveTool = new MoveTool(_mockDesktopService.Object, _mockMoveLogger.Object);

            // Act
            var result = await moveTool.MoveAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(s => s.MoveAsync(x, y), Times.Once);
        }

        [Fact]
        public async Task DragTool_DragAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Drag operation completed";
            _mockDesktopService.Setup(x => x.DragAsync(100, 200, 300, 400))
                              .ReturnsAsync(expectedResult);
            var dragTool = new DragTool(_mockDesktopService.Object, _mockDragLogger.Object);

            // Act
            var result = await dragTool.DragAsync(100, 200, 300, 400);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.DragAsync(100, 200, 300, 400), Times.Once);
        }

        [Fact]
        public async Task ScrollTool_ScrollAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Scroll completed";
            _mockDesktopService.Setup(x => x.ScrollAsync(null, null, "vertical", "down", 1))
                              .ReturnsAsync(expectedResult);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockScrollLogger.Object);

            // Act
            var result = await scrollTool.ScrollAsync();

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrollAsync(null, null, "vertical", "down", 1), Times.Once);
        }

        [Theory]
        [InlineData("vertical", "up", 3)]
        [InlineData("horizontal", "left", 2)]
        [InlineData("vertical", "down", 5)]
        public async Task ScrollTool_ScrollAsync_WithParameters_ShouldCallService(string type, string direction, int wheelTimes)
        {
            // Arrange
            var expectedResult = $"Scrolled {direction} {wheelTimes} times";
            _mockDesktopService.Setup(x => x.ScrollAsync(100, 200, type, direction, wheelTimes))
                              .ReturnsAsync(expectedResult);
            var scrollTool = new ScrollTool(_mockDesktopService.Object, _mockScrollLogger.Object);

            // Act
            var result = await scrollTool.ScrollAsync(100, 200, type, direction, wheelTimes);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrollAsync(100, 200, type, direction, wheelTimes), Times.Once);
        }

        [Fact]
        public async Task KeyTool_KeyAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Key pressed successfully";
            _mockDesktopService.Setup(x => x.KeyAsync("Enter"))
                              .ReturnsAsync(expectedResult);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockKeyLogger.Object);

            // Act
            var result = await keyTool.KeyAsync("Enter");

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.KeyAsync("Enter"), Times.Once);
        }

        [Theory]
        [InlineData("Escape")]
        [InlineData("Tab")]
        [InlineData("Space")]
        [InlineData("F1")]
        public async Task KeyTool_KeyAsync_WithDifferentKeys_ShouldCallService(string key)
        {
            // Arrange
            var expectedResult = $"Pressed {key}";
            _mockDesktopService.Setup(x => x.KeyAsync(key))
                              .ReturnsAsync(expectedResult);
            var keyTool = new KeyTool(_mockDesktopService.Object, _mockKeyLogger.Object);

            // Act
            var result = await keyTool.KeyAsync(key);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.KeyAsync(key), Times.Once);
        }

        [Fact]
        public async Task ShortcutTool_ShortcutAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var keys = new[] { "Ctrl", "C" };
            var expectedResult = "Shortcut executed successfully";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockShortcutLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task ShortcutTool_ShortcutAsync_WithCtrlV_ShouldCallService()
        {
            // Arrange
            var keys = new[] { "Ctrl", "V" };
            var expectedResult = $"Executed shortcut: {string.Join("+", keys)}";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockShortcutLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task ShortcutTool_ShortcutAsync_WithAltTab_ShouldCallService()
        {
            // Arrange
            var keys = new[] { "Alt", "Tab" };
            var expectedResult = $"Executed shortcut: {string.Join("+", keys)}";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockShortcutLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task ShortcutTool_ShortcutAsync_WithCtrlShiftN_ShouldCallService()
        {
            // Arrange
            var keys = new[] { "Ctrl", "Shift", "N" };
            var expectedResult = $"Executed shortcut: {string.Join("+", keys)}";
            _mockDesktopService.Setup(x => x.ShortcutAsync(keys))
                              .ReturnsAsync(expectedResult);
            var shortcutTool = new ShortcutTool(_mockDesktopService.Object, _mockShortcutLogger.Object);

            // Act
            var result = await shortcutTool.ShortcutAsync(keys);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ShortcutAsync(keys), Times.Once);
        }

        [Fact]
        public async Task WaitTool_WaitAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var expectedResult = "Wait completed";
            _mockDesktopService.Setup(x => x.WaitAsync(5))
                              .ReturnsAsync(expectedResult);
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockWaitLogger.Object);

            // Act
            var result = await waitTool.WaitAsync(5);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.WaitAsync(5), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(30)]
        public async Task WaitTool_WaitAsync_WithDifferentDurations_ShouldCallService(int duration)
        {
            // Arrange
            var expectedResult = $"Waited for {duration} seconds";
            _mockDesktopService.Setup(x => x.WaitAsync(duration))
                              .ReturnsAsync(expectedResult);
            var waitTool = new WaitTool(_mockDesktopService.Object, _mockWaitLogger.Object);

            // Act
            var result = await waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.WaitAsync(duration), Times.Once);
        }

        [Fact]
        public async Task ScreenshotTool_TakeScreenshotAsync_ShouldReturnImagePath()
        {
            // Arrange
            var expectedResult = "C:\\temp\\screenshot.png";
            _mockDesktopService.Setup(x => x.TakeScreenshotAsync())
                              .ReturnsAsync(expectedResult);
            var screenshotTool = new ScreenshotTool(_mockDesktopService.Object, _mockScreenshotLogger.Object);

            // Act
            var result = await screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.TakeScreenshotAsync(), Times.Once);
        }

        [Fact]
        public async Task LaunchTool_LaunchAppAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var appName = "notepad";
            var expectedResult = "Application launched successfully";
            _mockDesktopService.Setup(x => x.LaunchAppAsync(appName))
                              .ReturnsAsync((expectedResult, 0));
            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLaunchLogger.Object);

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
        public async Task LaunchTool_LaunchAppAsync_WithDifferentApps_ShouldCallService(string appName)
        {
            // Arrange
            var expectedResult = $"Launched {appName}";
            _mockDesktopService.Setup(x => x.LaunchAppAsync(appName))
                              .ReturnsAsync((expectedResult, 0));
            var launchTool = new LaunchTool(_mockDesktopService.Object, _mockLaunchLogger.Object);

            // Act
            var result = await launchTool.LaunchAppAsync(appName);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.LaunchAppAsync(appName), Times.Once);
        }

        [Fact]
        public async Task SwitchTool_SwitchAppAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var appName = "Notepad";
            var expectedResult = "Switched to application successfully";
            _mockDesktopService.Setup(x => x.SwitchAppAsync(appName))
                              .ReturnsAsync((expectedResult, 0));
            var switchTool = new SwitchTool(_mockDesktopService.Object, _mockSwitchLogger.Object);

            // Act
            var result = await switchTool.SwitchAppAsync(appName);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.SwitchAppAsync(appName), Times.Once);
        }

        [Fact]
        public async Task ResizeTool_ResizeAppAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var appName = "Notepad";
            var expectedResult = "Window resized successfully";
            _mockDesktopService.Setup(x => x.ResizeAppAsync(appName, 800, 600, null, null))
                              .ReturnsAsync((expectedResult, 0));
            var resizeTool = new ResizeTool(_mockDesktopService.Object, _mockResizeLogger.Object);

            // Act
            var result = await resizeTool.ResizeAppAsync(appName, 800, 600);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ResizeAppAsync(appName, 800, 600, null, null), Times.Once);
        }

        [Fact]
        public async Task GetWindowInfoTool_GetWindowInfoAsync_ShouldReturnWindowInfo()
        {
            // Arrange
            var appName = "Notepad";
            var expectedResult = "{\"width\": 800, \"height\": 600, \"x\": 100, \"y\": 100}";
            _mockDesktopService.Setup(x => x.GetWindowInfoAsync(appName))
                              .ReturnsAsync((expectedResult, 0));
            var windowInfoTool = new GetWindowInfoTool(_mockDesktopService.Object, _mockWindowInfoLogger.Object);

            // Act
            var result = await windowInfoTool.GetWindowInfoAsync(appName);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.GetWindowInfoAsync(appName), Times.Once);
        }

        [Fact]
        public async Task StateTool_GetDesktopStateAsync_ShouldReturnDesktopState()
        {
            // Arrange
            var expectedResult = "{\"applications\": [], \"activeWindow\": \"Desktop\"}";
            _mockDesktopService.Setup(x => x.GetDesktopStateAsync(false))
                               .ReturnsAsync(expectedResult);
            var stateTool = new StateTool(_mockDesktopService.Object, _mockStateLogger.Object);

            // Act
            var result = await stateTool.GetDesktopStateAsync();

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.GetDesktopStateAsync(false), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task StateTool_GetDesktopStateAsync_WithVision_ShouldCallService(bool useVision)
        {
            // Arrange
            var expectedResult = $"Desktop state with vision: {useVision}";
            _mockDesktopService.Setup(x => x.GetDesktopStateAsync(useVision))
                               .ReturnsAsync(expectedResult);
            var stateTool = new StateTool(_mockDesktopService.Object, _mockStateLogger.Object);

            // Act
            var result = await stateTool.GetDesktopStateAsync(useVision);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.GetDesktopStateAsync(useVision), Times.Once);
        }

        [Fact]
        public async Task PowershellTool_ExecuteCommandAsync_ShouldReturnCommandOutput()
        {
            // Arrange
            var command = "Get-Date";
            var expectedResult = "2024-01-01 12:00:00";
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(command))
                              .ReturnsAsync((expectedResult, 0));
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockPowershellLogger.Object);

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
        public async Task PowershellTool_ExecuteCommandAsync_WithDifferentCommands_ShouldCallService(string command)
        {
            // Arrange
            var expectedResult = $"Output of {command}";
            _mockDesktopService.Setup(x => x.ExecuteCommandAsync(command))
                              .ReturnsAsync((expectedResult, 0));
            var powershellTool = new PowershellTool(_mockDesktopService.Object, _mockPowershellLogger.Object);

            // Act
            var result = await powershellTool.ExecuteCommandAsync(command);

            // Assert
            var expectedOutput = $"Status Code: 0\nResponse: {expectedResult}";
            Assert.Equal(expectedOutput, result);
            _mockDesktopService.Verify(x => x.ExecuteCommandAsync(command), Times.Once);
        }

        [Fact]
        public async Task ScrapeTool_ScrapeAsync_ShouldReturnMarkdownContent()
        {
            // Arrange
            var url = "https://example.com";
            var expectedResult = "# Example\n\nThis is example content.";
            _mockDesktopService.Setup(x => x.ScrapeAsync(url))
                               .ReturnsAsync(expectedResult);
            var scrapeTool = new ScrapeTool(_mockDesktopService.Object, _mockScrapeLogger.Object);

            // Act
            var result = await scrapeTool.ScrapeAsync(url);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.ScrapeAsync(url), Times.Once);
        }

        [Fact]
        public async Task OpenBrowserTool_OpenBrowserAsync_ShouldReturnSuccessMessage()
        {
            // Arrange
            var url = "https://google.com";
            var expectedResult = "Browser opened successfully";
            _mockDesktopService.Setup(x => x.OpenBrowserAsync(url))
                               .ReturnsAsync(expectedResult);
            var browserTool = new OpenBrowserTool(_mockDesktopService.Object, _mockBrowserLogger.Object);

            // Act
            var result = await browserTool.OpenBrowserAsync(url);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.OpenBrowserAsync(url), Times.Once);
        }
    }
}