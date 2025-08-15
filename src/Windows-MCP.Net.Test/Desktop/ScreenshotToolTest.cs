using Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Tools.Desktop;
using WindowsMCP.Net.Services;
using Xunit;

namespace Windows_MCP.Net.Test.Desktop
{
    /// <summary>
    /// ScreenshotTool单元测试类
    /// </summary>
    public class ScreenshotToolTest
    {
        private readonly IDesktopService _desktopService;
        private readonly ILogger<ScreenshotTool> _logger;
        private readonly ScreenshotTool _screenshotTool;

        public ScreenshotToolTest()
        {
            // 创建服务容器并注册依赖
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IDesktopService, DesktopService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // 获取实际的服务实例
            _desktopService = serviceProvider.GetRequiredService<IDesktopService>();
            _logger = serviceProvider.GetRequiredService<ILogger<ScreenshotTool>>();
            _screenshotTool = new ScreenshotTool(_desktopService, _logger);
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldReturnImagePath()
        {
            // Act
            var result = await _screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(File.Exists(result), "Screenshot file should exist");
            Assert.True(result.EndsWith(".png"), "Screenshot should be a PNG file");
            
            // 清理测试文件
            if (File.Exists(result))
            {
                File.Delete(result);
            }
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldReturnValidFilePath()
        {
            // Act
            var result = await _screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("screenshot", result.ToLower());
            Assert.True(result.EndsWith(".png") || result.EndsWith(".jpg") || result.EndsWith(".jpeg"));
            Assert.True(Path.IsPathFullyQualified(result), "Should return absolute path");
            Assert.True(File.Exists(result), "Screenshot file should exist");
            
            // 清理测试文件
            if (File.Exists(result))
            {
                File.Delete(result);
            }
        }

        [Fact]
        public async Task TakeScreenshotAsync_MultipleConsecutiveCalls_ShouldReturnDifferentFiles()
        {
            var results = new List<string>();

            // Act - 连续调用3次截图
            for (int i = 0; i < 3; i++)
            {
                var result = await _screenshotTool.TakeScreenshotAsync();
                results.Add(result);
                
                // 每次调用之间稍作延迟，确保文件名不同
                await Task.Delay(1000);
            }

            // Assert
            Assert.Equal(3, results.Count);
            foreach (var result in results)
            {
                Assert.NotNull(result);
                Assert.True(File.Exists(result), $"Screenshot file should exist: {result}");
                Assert.True(result.EndsWith(".png"), "All screenshots should be PNG files");
            }
            
            // 验证每个文件都是不同的
            Assert.Equal(3, results.Distinct().Count());
            
            // 清理测试文件
            foreach (var result in results)
            {
                if (File.Exists(result))
                {
                    File.Delete(result);
                }
            }
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldCreateFileInTempDirectory()
        {
            // Act
            var result = await _screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            var tempPath = Path.GetTempPath();
            Assert.StartsWith(tempPath, result);
            Assert.True(File.Exists(result), "Screenshot file should exist");
            
            // 清理测试文件
            if (File.Exists(result))
            {
                File.Delete(result);
            }
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldHandleExceptionsGracefully()
        {
            // 这个测试验证在正常情况下不会抛出异常
            // 如果需要测试异常情况，可以通过模拟系统故障来实现
            
            // Act & Assert - 正常情况下应该成功
            var result = await _screenshotTool.TakeScreenshotAsync();
            
            Assert.NotNull(result);
            Assert.True(File.Exists(result), "Screenshot should be created successfully");
            
            // 清理测试文件
            if (File.Exists(result))
            {
                File.Delete(result);
            }
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldReturnUniqueFileNames()
        {
            // Act - 连续截图两次，中间添加延迟确保时间戳不同
            var result1 = await _screenshotTool.TakeScreenshotAsync();
            await Task.Delay(1100); // 等待超过1秒确保时间戳不同
            var result2 = await _screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.True(File.Exists(result1), "First screenshot should exist");
            Assert.True(File.Exists(result2), "Second screenshot should exist");
            
            // 验证文件名是唯一的
            Assert.NotEqual(result1, result2);
            
            // 验证文件名包含特定字符串
            Assert.Contains("screenshot_", Path.GetFileName(result1));
            Assert.Contains("screenshot_", Path.GetFileName(result2));
            
            // 清理测试文件
            if (File.Exists(result1))
            {
                File.Delete(result1);
            }
            if (File.Exists(result2))
            {
                File.Delete(result2);
            }
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldReturnValidPath()
        {
            // Act
            var result = await _screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(Path.IsPathFullyQualified(result), "Should return absolute path");
            Assert.True(File.Exists(result), "Screenshot file should exist");
            
            // 清理测试文件
            if (File.Exists(result))
            {
                File.Delete(result);
            }
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldCreatePngFormat()
        {
            // Act
            var result = await _screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("screenshot", result);
            Assert.True(result.EndsWith(".png"), "Screenshot should be in PNG format");
            Assert.True(File.Exists(result), "Screenshot file should exist");
            
            // 验证文件确实是有效的图片文件
            var fileInfo = new FileInfo(result);
            Assert.True(fileInfo.Length > 0, "Screenshot file should not be empty");
            
            // 清理测试文件
            if (File.Exists(result))
            {
                File.Delete(result);
            }
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldExecuteSuccessfully()
        {
            // Act
            var result = await _screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(File.Exists(result), "Screenshot should be created successfully");
            Assert.Contains("screenshot", Path.GetFileName(result).ToLower());
            
            // 清理测试文件
            if (File.Exists(result))
            {
                File.Delete(result);
            }
        }

        [Fact]
        public async Task TakeScreenshotAsync_ShouldCreateFileWithTimestamp()
        {
            // Act
            var result = await _screenshotTool.TakeScreenshotAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result), "Screenshot file should exist");
            
            var fileName = Path.GetFileName(result);
            Assert.Contains("screenshot_", fileName);
            Assert.True(fileName.EndsWith(".png"), "File should have PNG extension");
            
            // 验证文件名包含时间戳格式 (yyyyMMdd_HHmmss)
            var timestampPart = fileName.Replace("screenshot_", "").Replace(".png", "");
            Assert.True(timestampPart.Length >= 15, "Timestamp should be in yyyyMMdd_HHmmss format");
            
            // 清理测试文件
            if (File.Exists(result))
            {
                File.Delete(result);
            }
        }
    }
}
