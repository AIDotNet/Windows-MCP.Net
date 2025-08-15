using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// ClipboardTool单元测试类
    /// </summary>
    public class ClipboardToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<ClipboardTool> _logger;
        private readonly ClipboardTool _clipboardTool;

        public ClipboardToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<ClipboardTool>>();
            _clipboardTool = new ClipboardTool(_desktopService, _logger);
        }

        [Fact]
        public async Task ClipboardAsync_CopyMode_ShouldReturnSuccessMessage()
        {
            // Act
            var result = await _clipboardTool.ClipboardAsync("copy", "Test text");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ClipboardAsync_PasteMode_ShouldReturnClipboardContent()
        {
            // Act
            var result = await _clipboardTool.ClipboardAsync("paste");

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("Hello World")]
        [InlineData("")]
        [InlineData("Special chars: @#$%^&*()")]
        [InlineData("Multi\nLine\nText")]
        [InlineData("中文内容测试")]
        public async Task ClipboardAsync_CopyWithDifferentTexts_ShouldCallService(string text)
        {
            // Act
            var result = await _clipboardTool.ClipboardAsync("copy", text);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ClipboardAsync_PasteWithoutText_ShouldCallServiceWithNull()
        {
            // Act
            var result = await _clipboardTool.ClipboardAsync("paste", null);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ClipboardAsync_CopyLongText_ShouldCallService()
        {
            // Arrange
            var longText = new string('A', 5000); // 5000字符的长文本

            // Act
            var result = await _clipboardTool.ClipboardAsync("copy", longText);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("copy")]
        [InlineData("paste")]
        [InlineData("COPY")]
        [InlineData("PASTE")]
        public async Task ClipboardAsync_WithDifferentModes_ShouldCallService(string mode)
        {
            // Arrange
            var text = mode.ToLower() == "copy" ? "Sample text" : null;

            // Act
            var result = await _clipboardTool.ClipboardAsync(mode, text);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ClipboardAsync_CopyEmptyString_ShouldCallService()
        {
            // Act
            var result = await _clipboardTool.ClipboardAsync("copy", "");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ClipboardAsync_ValidOperation_ShouldNotThrowException()
        {
            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _clipboardTool.ClipboardAsync("copy", "test");
            });
            
            // 正常情况下不应该抛出异常
            Assert.Null(exception);
        }

        [Fact]
        public async Task ClipboardAsync_ConsecutiveOperations_ShouldWorkCorrectly()
        {
            // Act
            var copyResult = await _clipboardTool.ClipboardAsync("copy", "test");
            var pasteResult = await _clipboardTool.ClipboardAsync("paste");

            // Assert
            Assert.NotNull(copyResult);
            Assert.NotNull(pasteResult);
            // 验证粘贴的内容包含我们刚才复制的文本
            Assert.Contains("test", pasteResult);
        }
    }
}
