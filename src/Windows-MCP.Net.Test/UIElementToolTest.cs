using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test
{
    /// <summary>
    /// UI元素识别工具测试类
    /// </summary>
    public class UIElementToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<UIElementTool> _logger;
        private readonly UIElementTool _uiElementTool;

        public UIElementToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<UIElementTool>>();
            _uiElementTool = new UIElementTool(_desktopService, _logger);
        }

        [Fact]
        public async Task FindElementByTextAsync_ShouldReturnValidJson_WhenCalled()
        {
            // Arrange
            var searchText = "OK";

            // Act
            var result = await _uiElementTool.FindElementByTextAsync(searchText);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            // 验证返回的是有效的JSON
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(successProperty.GetBoolean());
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
        }

        [Fact]
        public async Task FindElementByTextAsync_ShouldHandleNonExistentElement()
        {
            // Arrange
            var searchText = "NonExistentButton_" + Guid.NewGuid().ToString(); // 使用随机文本确保元素不存在

            // Act
            var result = await _uiElementTool.FindElementByTextAsync(searchText);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            // 即使元素未找到，操作也应该成功完成
            Assert.True(successProperty.GetBoolean());
            
            // 验证found属性为false
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.False(foundProperty.GetBoolean());
        }

        [Fact]
        public async Task FindElementByClassNameAsync_ShouldReturnValidJson_WhenCalled()
        {
            // Arrange
            var className = "Button";

            // Act
            var result = await _uiElementTool.FindElementByClassNameAsync(className);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            // 验证返回的是有效的JSON
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(successProperty.GetBoolean());
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
        }

        [Fact]
        public async Task FindElementByAutomationIdAsync_ShouldReturnValidJson_WhenCalled()
        {
            // Arrange
            var automationId = "NonExistent_" + Guid.NewGuid().ToString();

            // Act
            var result = await _uiElementTool.FindElementByAutomationIdAsync(automationId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            // 验证返回的是有效的JSON
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(successProperty.GetBoolean());
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
        }

        [Fact]
        public async Task GetElementPropertiesAsync_ShouldReturnValidJson_WhenCalled()
        {
            // Arrange
            var x = 100;
            var y = 100;

            // Act
            var result = await _uiElementTool.GetElementPropertiesAsync(x, y);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            // 验证返回的是有效的JSON
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(successProperty.GetBoolean());
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("coordinates", out var coordinatesProperty));
        }

        [Fact]
        public async Task GetElementPropertiesAsync_ShouldHandleEmptyArea()
        {
            // Arrange - 使用屏幕外的坐标确保没有元素
            var x = 5000;
            var y = 5000;

            // Act
            var result = await _uiElementTool.GetElementPropertiesAsync(x, y);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(successProperty.GetBoolean());
            Assert.True(jsonResult.TryGetProperty("coordinates", out var coordinatesProperty));
        }

        [Theory]
        [InlineData("NonExistent1", "text", 500)]
        [InlineData("NonExistent2", "className", 500)]
        [InlineData("NonExistent3", "automationId", 500)]
        public async Task WaitForElementAsync_ShouldReturnValidJson_WhenCalled(string selector, string selectorType, int timeout)
        {
            // Arrange - 使用不存在的元素和短超时时间
            var uniqueSelector = selector + "_" + Guid.NewGuid().ToString();

            // Act
            var result = await _uiElementTool.WaitForElementAsync(uniqueSelector, selectorType, timeout);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(successProperty.GetBoolean());
            Assert.True(jsonResult.TryGetProperty("selector", out var selectorProperty));
            Assert.Equal(uniqueSelector, selectorProperty.GetString());
            Assert.True(jsonResult.TryGetProperty("selectorType", out var selectorTypeProperty));
            Assert.Equal(selectorType, selectorTypeProperty.GetString());
        }

        [Fact]
        public async Task WaitForElementAsync_ShouldHandleTimeout()
        {
            // Arrange
            var selector = "NonExistentElement_" + Guid.NewGuid().ToString();
            var selectorType = "text";
            var timeout = 500; // 使用短超时时间

            // Act
            var result = await _uiElementTool.WaitForElementAsync(selector, selectorType, timeout);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(successProperty.GetBoolean());
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            // 由于元素不存在且超时时间短，应该返回未找到
            Assert.False(foundProperty.GetBoolean());
        }

        [Fact]
        public async Task WaitForElementAsync_ShouldUseDefaultTimeout_WhenTimeoutNotSpecified()
        {
            // Arrange
            var selector = "NonExistentButton_" + Guid.NewGuid().ToString();
            var selectorType = "text";

            // Act - 不指定超时时间，使用默认值
            var result = await _uiElementTool.WaitForElementAsync(selector, selectorType);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(successProperty.GetBoolean());
            Assert.True(jsonResult.TryGetProperty("selector", out var selectorProperty));
            Assert.Equal(selector, selectorProperty.GetString());
            Assert.True(jsonResult.TryGetProperty("selectorType", out var selectorTypeProperty));
            Assert.Equal(selectorType, selectorTypeProperty.GetString());
        }
    }
}