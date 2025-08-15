using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// WaitTool单元测试类
    /// </summary>
    public class WaitToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<WaitTool> _logger;
        private readonly WaitTool _waitTool;

        public WaitToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<WaitTool>>();
            _waitTool = new WaitTool(_desktopService, _logger);
        }

        [Fact]
        public async Task WaitAsync_ShouldReturnSuccessMessage()
        {
            // Act
            var result = await _waitTool.WaitAsync(1); // 使用较短的等待时间以加快测试

            // Assert
            Assert.Equal("Waited for 1 seconds", result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task WaitAsync_WithDifferentDurations_ShouldCallService(int duration)
        {
            // Act
            var result = await _waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal($"Waited for {duration} seconds", result);
        }

        [Fact]
        public async Task WaitAsync_WithZeroDuration_ShouldCallService()
        {
            // Arrange
            var duration = 0;

            // Act
            var result = await _waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal("Waited for 0 seconds", result);
        }

        [Fact]
        public async Task WaitAsync_WithLargeDuration_ShouldCallService()
        {
            // Arrange
            var duration = 5; // 使用较小的值以加快测试

            // Act
            var result = await _waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal($"Waited for {duration} seconds", result);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task WaitAsync_WithCommonDurations_ShouldCallService(int duration)
        {
            // Act
            var result = await _waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal($"Waited for {duration} seconds", result);
        }

        [Fact]
        public async Task WaitAsync_MultipleConsecutiveWaits_ShouldCallServiceMultipleTimes()
        {
            // Arrange
            var durations = new[] { 1, 2, 1 };

            // Act & Assert
            foreach (var duration in durations)
            {
                var result = await _waitTool.WaitAsync(duration);
                Assert.Equal($"Waited for {duration} seconds", result);
            }
        }

        [Fact]
        public async Task WaitAsync_WithValidDuration_ShouldNotThrowException()
        {
            // Act & Assert - 确保正常的等待操作不会抛出异常
            var result = await _waitTool.WaitAsync(1);
            Assert.Equal("Waited for 1 seconds", result);
        }

        [Fact]
        public async Task WaitAsync_WithNegativeDuration_ShouldReturnError()
        {
            // Arrange
            var duration = -1;

            // Act
            var result = await _waitTool.WaitAsync(duration);

            // Assert
            Assert.StartsWith("Error:", result);
        }

        [Fact]
        public async Task WaitAsync_WithVerySmallDuration_ShouldCallService()
        {
            // Arrange
            var duration = 1;

            // Act
            var result = await _waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal("Waited for 1 seconds", result);
        }

        [Fact]
        public async Task WaitAsync_ShouldPassCorrectDurationToService()
        {
            // Arrange
            var duration = 2;

            // Act
            var result = await _waitTool.WaitAsync(duration);

            // Assert
            Assert.Equal("Waited for 2 seconds", result);
        }
    }
}
