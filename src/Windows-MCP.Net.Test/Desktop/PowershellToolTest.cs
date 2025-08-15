using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// PowershellTool单元测试类
    /// </summary>
    public class PowershellToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<PowershellTool> _logger;
        private readonly PowershellTool _powershellTool;

        public PowershellToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<PowershellTool>>();
            _powershellTool = new PowershellTool(_desktopService, _logger);
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldReturnCommandOutput()
        {
            // Arrange
            var command = "Get-Date";

            // Act
            var result = await _powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Status Code:", result);
            Assert.Contains("Response:", result);
        }

        [Theory]
        [InlineData("Get-Process")]
        [InlineData("Get-Location")]
        [InlineData("ls")]
        public async Task ExecuteCommandAsync_WithDifferentCommands_ShouldCallService(string command)
        {
            // Act
            var result = await _powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Status Code:", result);
            Assert.Contains("Response:", result);
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithErrorCommand_ShouldReturnErrorStatus()
        {
            // Arrange
            var command = "Get-NonExistentCommand";

            // Act
            var result = await _powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Status Code:", result);
            Assert.Contains("Response:", result);
            // 对于无效命令，状态码应该不是0
            Assert.DoesNotContain("Status Code: 0", result);
        }

        [Theory]
        [InlineData("Get-ChildItem")]
        [InlineData("Get-Process | Select-Object Name")]
        public async Task ExecuteCommandAsync_WithVariousCommands_ShouldCallService(string command)
        {
            // Act
            var result = await _powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Status Code:", result);
            Assert.Contains("Response:", result);
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithComplexCommand_ShouldCallService()
        {
            // Arrange
            var command = "Get-Process | Where-Object {$_.ProcessName -eq 'notepad'} | Select-Object Id, ProcessName";

            // Act
            var result = await _powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Status Code:", result);
            Assert.Contains("Response:", result);
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithMultilineOutput_ShouldReturnFormattedResult()
        {
            // Arrange
            var command = "Get-Service";

            // Act
            var result = await _powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Status Code:", result);
            Assert.Contains("Response:", result);
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithEmptyOutput_ShouldReturnFormattedResult()
        {
            // Arrange
            var command = "Clear-Host";

            // Act
            var result = await _powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Status Code:", result);
            Assert.Contains("Response:", result);
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithValidCommand_ShouldReturnCorrectFormat()
        {
            // Arrange
            var command = "Get-Date";

            // Act
            var result = await _powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Status Code:", result);
            Assert.Contains("Response:", result);
            // 验证格式正确
            var lines = result.Split('\n');
            Assert.True(lines.Length >= 2);
            Assert.StartsWith("Status Code:", lines[0]);
            Assert.StartsWith("Response:", lines[1]);
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithEmptyCommand_ShouldHandleGracefully()
        {
            // Arrange
            var command = "";

            // Act
            var result = await _powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Status Code:", result);
            Assert.Contains("Response:", result);
        }

        [Fact]
        public async Task ExecuteCommandAsync_ConsecutiveCommands_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var commands = new[]
            {
                "Get-Date",
                "Get-Location",
                "Get-Process"
            };

            // Act & Assert
            foreach (var command in commands)
            {
                var result = await _powershellTool.ExecuteCommandAsync(command);
                Assert.NotNull(result);
                Assert.Contains("Status Code:", result);
                Assert.Contains("Response:", result);
            }
        }

        [Fact]
        public async Task ExecuteCommandAsync_WithSpecialCharacters_ShouldCallService()
        {
            // Arrange
            var command = "Write-Output 'Hello, 世界! @#$%^&*()'";

            // Act
            var result = await _powershellTool.ExecuteCommandAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Status Code:", result);
            Assert.Contains("Response:", result);
            // 验证特殊字符被正确处理
            Assert.Contains("Hello, 世界!", result);
        }
    }
}
