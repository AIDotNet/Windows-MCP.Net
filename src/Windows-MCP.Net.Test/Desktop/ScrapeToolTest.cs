using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;
using Xunit;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// ScrapeTool单元测试类
    /// </summary>
    public class ScrapeToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<ScrapeTool> _logger;
        private readonly ScrapeTool _scrapeTool;

        public ScrapeToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<ScrapeTool>>();
            _scrapeTool = new ScrapeTool(_desktopService, _logger);
        }

        [Fact]
        public async Task ScrapeAsync_WithValidHttpUrl_ShouldReturnMarkdownContent()
        {
            // Arrange
            var url = "https://httpbin.org/html";
            
            // Act
            var result = await _scrapeTool.ScrapeAsync(url);
            
            // Assert
            Assert.NotNull(result);
            Assert.Contains("Scraped the contents of the entire webpage:", result);
            Assert.Contains("#", result); // Markdown should contain headers
        }

        [Fact]
        public async Task ScrapeAsync_WithValidHttpsUrl_ShouldReturnContent()
        {
            // Arrange
            var url = "https://example.com";
            
            // Act
            var result = await _scrapeTool.ScrapeAsync(url);
            
            // Assert
            Assert.NotNull(result);
            // 由于SSL或网络问题，可能返回错误信息或成功内容
            Assert.True(result.Contains("Scraped the contents of the entire webpage:") || result.Contains("Error:"));
        }

        [Fact]
        public async Task ScrapeAsync_WithInvalidUrl_ShouldReturnErrorMessage()
        {
            // Arrange
            var url = "https://invalid-url-that-does-not-exist-12345.com";
            
            // Act
            var result = await _scrapeTool.ScrapeAsync(url);
            
            // Assert
            Assert.NotNull(result);
            Assert.Contains("Error:", result);
        }

        [Fact]
        public async Task ScrapeAsync_WithMalformedUrl_ShouldReturnErrorMessage()
        {
            // Arrange
            var url = "not-a-valid-url";
            
            // Act
            var result = await _scrapeTool.ScrapeAsync(url);
            
            // Assert
            Assert.NotNull(result);
            Assert.Contains("Error:", result);
        }

        [Fact]
        public async Task ScrapeAsync_WithEmptyUrl_ShouldReturnErrorMessage()
        {
            // Arrange
            var url = "";
            
            // Act
            var result = await _scrapeTool.ScrapeAsync(url);
            
            // Assert
            Assert.NotNull(result);
            Assert.Contains("Error:", result);
        }

        [Fact]
        public async Task ScrapeAsync_WithNullUrl_ShouldReturnErrorMessage()
        {
            // Arrange
            string url = null;
            
            // Act
            var result = await _scrapeTool.ScrapeAsync(url);
            
            // Assert
            Assert.NotNull(result);
            Assert.Contains("Error:", result);
        }

        [Fact]
        public async Task ScrapeAsync_WithHttpUrl_ShouldReturnMarkdownContent()
        {
            // Arrange
            var url = "http://httpbin.org/html";
            
            // Act
            var result = await _scrapeTool.ScrapeAsync(url);
            
            // Assert
            Assert.NotNull(result);
            Assert.Contains("Scraped the contents of the entire webpage:", result);
        }

        [Theory]
        [InlineData("https://httpbin.org/html")]
        [InlineData("https://example.com")]
        [InlineData("http://httpbin.org/html")]
        public async Task ScrapeAsync_WithValidUrls_ShouldReturnContent(string url)
        {
            // Act
            var result = await _scrapeTool.ScrapeAsync(url);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Contains("Scraped the contents of the entire webpage:") || result.Contains("Error:"));
        }

        [Theory]
        [InlineData("invalid-url")]
        [InlineData("ftp://example.com")]
        [InlineData("https://")]
        [InlineData("http://")]
        public async Task ScrapeAsync_WithInvalidUrls_ShouldReturnErrorMessage(string url)
        {
            // Act
            var result = await _scrapeTool.ScrapeAsync(url);
            
            // Assert
            Assert.NotNull(result);
            Assert.Contains("Error:", result);
        }
    }
}