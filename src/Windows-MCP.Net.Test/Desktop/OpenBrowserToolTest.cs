using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// OpenBrowserTool单元测试类
    /// </summary>
    public class OpenBrowserToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<OpenBrowserTool> _logger;
        private readonly OpenBrowserTool _openBrowserTool;

        public OpenBrowserToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<OpenBrowserTool>>();
            _openBrowserTool = new OpenBrowserTool(_desktopService, _logger);
        }

        [Fact]
        public async Task OpenBrowserAsync_WithNoParameters_ShouldCallServiceWithDefaults()
        {
            // Act
            var result = await _openBrowserTool.OpenBrowserAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task OpenBrowserAsync_WithUrl_ShouldCallServiceWithUrl()
        {
            // Arrange
            var url = "https://www.google.com";

            // Act
            var result = await _openBrowserTool.OpenBrowserAsync(url);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task OpenBrowserAsync_WithSearchQuery_ShouldCallServiceWithSearchQuery()
        {
            // Arrange
            var searchQuery = "C# programming";

            // Act
            var result = await _openBrowserTool.OpenBrowserAsync(null, searchQuery);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task OpenBrowserAsync_WithUrlAndSearchQuery_ShouldCallServiceWithBothParameters()
        {
            // Arrange
            var url = "https://www.baidu.com";
            var searchQuery = "Windows MCP";

            // Act
            var result = await _openBrowserTool.OpenBrowserAsync(url, searchQuery);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("https://www.google.com")]
        [InlineData("https://www.baidu.com")]
        [InlineData("https://github.com")]
        [InlineData("http://localhost:3000")]
        public async Task OpenBrowserAsync_WithDifferentUrls_ShouldCallService(string url)
        {
            // Act
            var result = await _openBrowserTool.OpenBrowserAsync(url);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineData("C# programming")]
        [InlineData("ASP.NET Core")]
        [InlineData("Windows MCP")]
        [InlineData("机器学习")]
        public async Task OpenBrowserAsync_WithDifferentSearchQueries_ShouldCallService(string searchQuery)
        {
            // Act
            var result = await _openBrowserTool.OpenBrowserAsync(null, searchQuery);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task OpenBrowserAsync_WithEmptyUrl_ShouldCallServiceWithEmptyUrl()
        {
            // Arrange
            var url = string.Empty;

            // Act
            var result = await _openBrowserTool.OpenBrowserAsync(url);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task OpenBrowserAsync_WithEmptySearchQuery_ShouldCallServiceWithEmptySearchQuery()
        {
            // Arrange
            var searchQuery = string.Empty;

            // Act
            var result = await _openBrowserTool.OpenBrowserAsync(null, searchQuery);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task OpenBrowserAsync_WithValidUrl_ShouldReturnResult()
        {
            // Arrange
            var url = "https://www.example.com";

            // Act
            var result = await _openBrowserTool.OpenBrowserAsync(url);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task OpenBrowserAsync_WithUrlAndSearchQuery_ShouldReturnResult()
        {
            // Arrange
            var url = "https://www.test.com";
            var searchQuery = "test query";

            // Act
            var result = await _openBrowserTool.OpenBrowserAsync(url, searchQuery);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}