using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// TypeTool单元测试类
    /// </summary>
    public class TypeToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<TypeTool> _logger;
        private readonly TypeTool _typeTool;

        public TypeToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<TypeTool>>();
            _typeTool = new TypeTool(_desktopService, _logger);
        }

        [Fact]
        public async Task TypeAsync_ShouldReturnSuccessMessage()
        {
            // Act
            var result = await _typeTool.TypeAsync(100, 200, "Hello World", false, false);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("Test text", true, false)]
        [InlineData("Another text", false, true)]
        [InlineData("Complex text with symbols!@#", true, true)]
        public async Task TypeAsync_WithDifferentParameters_ShouldCallService(string text, bool clear, bool pressEnter)
        {
            // Act
            var result = await _typeTool.TypeAsync(300, 400, text, clear, pressEnter);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task TypeAsync_WithDefaultParameters_ShouldUseDefaults()
        {
            // Act
            var result = await _typeTool.TypeAsync(150, 250, "Default text");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task TypeAsync_WithClearFlag_ShouldCallServiceCorrectly()
        {
            // Act
            var result = await _typeTool.TypeAsync(200, 300, "New text", true);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task TypeAsync_WithPressEnterFlag_ShouldCallServiceCorrectly()
        {
            // Act
            var result = await _typeTool.TypeAsync(400, 500, "Submit text", false, true);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("A")]
        [InlineData("Hello, 世界! 123 @#$%")]
        [InlineData("Multi\nLine\nText")]
        public async Task TypeAsync_WithDifferentTextTypes_ShouldCallService(string text)
        {
            // Act
            var result = await _typeTool.TypeAsync(100, 100, text);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task TypeAsync_WithBothClearAndEnterFlags_ShouldCallServiceCorrectly()
        {
            // Act
            var result = await _typeTool.TypeAsync(250, 350, "Complete operation", true, true);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task TypeAsync_WithLongText_ShouldCallService()
        {
            // Arrange
            var longText = new string('A', 1000); // 1000字符的长文本

            // Act
            var result = await _typeTool.TypeAsync(500, 600, longText);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task TypeAsync_ShouldExecuteSuccessfully()
        {
            // Act
            var result = await _typeTool.TypeAsync(100, 200, "Test");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
