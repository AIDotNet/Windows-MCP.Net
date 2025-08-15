using Interface;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Tools.Desktop;

namespace Windows_MCP.Net.Test
{
    /// <summary>
    /// UI元素识别工具测试类
    /// </summary>
    public class UIElementToolTest
    {
        private readonly Mock<IDesktopService> _mockDesktopService;
        private readonly Mock<ILogger<UIElementTool>> _mockLogger;

        public UIElementToolTest()
        {
            _mockDesktopService = new Mock<IDesktopService>();
            _mockLogger = new Mock<ILogger<UIElementTool>>();
        }

        [Fact]
        public async Task FindElementByTextAsync_ShouldReturnElementInfo_WhenElementFound()
        {
            // Arrange
            var searchText = "OK";
            var expectedResult = JsonSerializer.Serialize(new
            {
                success = true,
                found = true,
                element = new
                {
                    name = "OK",
                    automationId = "btnOK",
                    className = "Button",
                    controlType = "button",
                    boundingRectangle = new { x = 100, y = 200, width = 80, height = 30 },
                    isEnabled = true,
                    isVisible = true
                }
            });
            
            _mockDesktopService.Setup(x => x.FindElementByTextAsync(searchText))
                              .ReturnsAsync(expectedResult);
            var uiElementTool = new UIElementTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await uiElementTool.FindElementByTextAsync(searchText);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.FindElementByTextAsync(searchText), Times.Once);
        }

        [Fact]
        public async Task FindElementByTextAsync_ShouldReturnNotFound_WhenElementNotExists()
        {
            // Arrange
            var searchText = "NonExistentButton";
            var expectedResult = JsonSerializer.Serialize(new
            {
                success = true,
                found = false,
                message = $"Element with text '{searchText}' not found"
            });
            
            _mockDesktopService.Setup(x => x.FindElementByTextAsync(searchText))
                              .ReturnsAsync(expectedResult);
            var uiElementTool = new UIElementTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await uiElementTool.FindElementByTextAsync(searchText);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.False(jsonResult.GetProperty("found").GetBoolean());
            _mockDesktopService.Verify(x => x.FindElementByTextAsync(searchText), Times.Once);
        }

        [Fact]
        public async Task FindElementByClassNameAsync_ShouldReturnElementInfo_WhenElementFound()
        {
            // Arrange
            var className = "Button";
            var expectedResult = JsonSerializer.Serialize(new
            {
                success = true,
                found = true,
                element = new
                {
                    name = "Submit",
                    automationId = "btnSubmit",
                    className = "Button",
                    controlType = "button",
                    boundingRectangle = new { x = 150, y = 250, width = 100, height = 35 },
                    isEnabled = true,
                    isVisible = true
                }
            });
            
            _mockDesktopService.Setup(x => x.FindElementByClassNameAsync(className))
                              .ReturnsAsync(expectedResult);
            var uiElementTool = new UIElementTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await uiElementTool.FindElementByClassNameAsync(className);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.FindElementByClassNameAsync(className), Times.Once);
        }

        [Fact]
        public async Task FindElementByAutomationIdAsync_ShouldReturnElementInfo_WhenElementFound()
        {
            // Arrange
            var automationId = "txtUsername";
            var expectedResult = JsonSerializer.Serialize(new
            {
                success = true,
                found = true,
                element = new
                {
                    name = "Username",
                    automationId = "txtUsername",
                    className = "TextBox",
                    controlType = "edit",
                    boundingRectangle = new { x = 200, y = 100, width = 150, height = 25 },
                    isEnabled = true,
                    isVisible = true
                }
            });
            
            _mockDesktopService.Setup(x => x.FindElementByAutomationIdAsync(automationId))
                              .ReturnsAsync(expectedResult);
            var uiElementTool = new UIElementTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await uiElementTool.FindElementByAutomationIdAsync(automationId);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(x => x.FindElementByAutomationIdAsync(automationId), Times.Once);
        }

        [Fact]
        public async Task GetElementPropertiesAsync_ShouldReturnElementProperties_WhenElementExists()
        {
            // Arrange
            var x = 300;
            var y = 400;
            var expectedResult = JsonSerializer.Serialize(new
            {
                success = true,
                found = true,
                coordinates = new { x, y },
                element = new
                {
                    name = "Close",
                    automationId = "btnClose",
                    className = "Button",
                    controlType = "button",
                    boundingRectangle = new { x = 290, y = 390, width = 20, height = 20 },
                    isEnabled = true,
                    isVisible = true,
                    hasKeyboardFocus = false,
                    isKeyboardFocusable = true
                }
            });
            
            _mockDesktopService.Setup(s => s.GetElementPropertiesAsync(x, y))
                              .ReturnsAsync(expectedResult);
            var uiElementTool = new UIElementTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await uiElementTool.GetElementPropertiesAsync(x, y);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockDesktopService.Verify(s => s.GetElementPropertiesAsync(x, y), Times.Once);
        }

        [Fact]
        public async Task GetElementPropertiesAsync_ShouldReturnNotFound_WhenNoElementAtCoordinates()
        {
            // Arrange
            var x = 1000;
            var y = 1000;
            var expectedResult = JsonSerializer.Serialize(new
            {
                success = true,
                found = false,
                coordinates = new { x, y },
                message = $"No UI element found at coordinates ({x}, {y})"
            });
            
            _mockDesktopService.Setup(s => s.GetElementPropertiesAsync(x, y))
                              .ReturnsAsync(expectedResult);
            var uiElementTool = new UIElementTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await uiElementTool.GetElementPropertiesAsync(x, y);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.False(jsonResult.GetProperty("found").GetBoolean());
            _mockDesktopService.Verify(s => s.GetElementPropertiesAsync(x, y), Times.Once);
        }

        [Theory]
        [InlineData("OK", "text", 1000)]
        [InlineData("Button", "className", 2000)]
        [InlineData("btnSubmit", "automationId", 3000)]
        public async Task WaitForElementAsync_ShouldReturnElementInfo_WhenElementAppears(string selector, string selectorType, int timeout)
        {
            // Arrange
            var expectedResult = JsonSerializer.Serialize(new
            {
                success = true,
                found = true,
                waitTime = 500,
                selector,
                selectorType,
                element = new
                {
                    name = "Test Element",
                    automationId = "testElement",
                    className = "TestClass",
                    controlType = "button",
                    boundingRectangle = new { x = 100, y = 100, width = 80, height = 30 },
                    isEnabled = true,
                    isVisible = true
                }
            });
            
            _mockDesktopService.Setup(s => s.WaitForElementAsync(selector, selectorType, timeout))
                              .ReturnsAsync(expectedResult);
            var uiElementTool = new UIElementTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await uiElementTool.WaitForElementAsync(selector, selectorType, timeout);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.True(jsonResult.GetProperty("found").GetBoolean());
            Assert.Equal(selector, jsonResult.GetProperty("selector").GetString());
            Assert.Equal(selectorType, jsonResult.GetProperty("selectorType").GetString());
            _mockDesktopService.Verify(s => s.WaitForElementAsync(selector, selectorType, timeout), Times.Once);
        }

        [Fact]
        public async Task WaitForElementAsync_ShouldReturnTimeout_WhenElementDoesNotAppear()
        {
            // Arrange
            var selector = "NonExistentElement";
            var selectorType = "text";
            var timeout = 1000;
            var expectedResult = JsonSerializer.Serialize(new
            {
                success = true,
                found = false,
                timeout = true,
                waitTime = timeout,
                selector,
                selectorType,
                message = $"Element with {selectorType} '{selector}' not found within {timeout}ms timeout"
            });
            
            _mockDesktopService.Setup(s => s.WaitForElementAsync(selector, selectorType, timeout))
                              .ReturnsAsync(expectedResult);
            var uiElementTool = new UIElementTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await uiElementTool.WaitForElementAsync(selector, selectorType, timeout);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.False(jsonResult.GetProperty("found").GetBoolean());
            Assert.True(jsonResult.GetProperty("timeout").GetBoolean());
            _mockDesktopService.Verify(s => s.WaitForElementAsync(selector, selectorType, timeout), Times.Once);
        }

        [Fact]
        public async Task WaitForElementAsync_ShouldUseDefaultTimeout_WhenTimeoutNotSpecified()
        {
            // Arrange
            var selector = "TestButton";
            var selectorType = "text";
            var defaultTimeout = 5000;
            var expectedResult = JsonSerializer.Serialize(new
            {
                success = true,
                found = true,
                waitTime = 100,
                selector,
                selectorType,
                element = new
                {
                    name = "TestButton",
                    automationId = "btnTest",
                    className = "Button",
                    controlType = "button",
                    boundingRectangle = new { x = 50, y = 50, width = 100, height = 30 },
                    isEnabled = true,
                    isVisible = true
                }
            });
            
            _mockDesktopService.Setup(s => s.WaitForElementAsync(selector, selectorType, defaultTimeout))
                              .ReturnsAsync(expectedResult);
            var uiElementTool = new UIElementTool(_mockDesktopService.Object, _mockLogger.Object);

            // Act
            var result = await uiElementTool.WaitForElementAsync(selector, selectorType);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.GetProperty("success").GetBoolean());
            Assert.True(jsonResult.GetProperty("found").GetBoolean());
            _mockDesktopService.Verify(s => s.WaitForElementAsync(selector, selectorType, defaultTimeout), Times.Once);
        }
    }
}