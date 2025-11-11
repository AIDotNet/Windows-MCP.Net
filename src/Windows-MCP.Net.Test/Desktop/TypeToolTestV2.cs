
using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    public class TypeToolTestV2
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<TypeTool> _logger;
        private readonly TypeTool _typeTool;

        public TypeToolTestV2()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();

            var serviceProvider = services.BuildServiceProvider();

            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<TypeTool>>();
            _typeTool = new TypeTool(_desktopService, _logger);
        }

        [Fact]
        public async Task TypeAsync_WithInterpretSpecialCharacters_ShouldReturnSuccessMessage()
        {
            // Act
            var result = await _typeTool.TypeAsync(100, 200, "Hello\nWorld", false, false, true);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
