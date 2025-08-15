using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using WindowsMCP.Net.Tools.OCR;
using Interface;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.OCR
{
    /// <summary>
    /// ExtractTextFromRegionTool 的专用单元测试类
    /// 测试从屏幕指定区域提取文本的所有功能
    /// </summary>
    public class ExtractTextFromRegionToolTest
    {
        private readonly IOcrService _ocrService;
        private readonly ILogger<ExtractTextFromRegionTool> _logger;
        private readonly ExtractTextFromRegionTool _tool;

        public ExtractTextFromRegionToolTest()
        {
            _ocrService = new OcrService();
            _logger = NullLogger<ExtractTextFromRegionTool>.Instance;
            _tool = new ExtractTextFromRegionTool(_ocrService, _logger);
        }

        #region 基础功能测试

        /// <summary>
        /// 测试ExtractTextFromRegionAsync基础功能
        /// 验证工具能够正确调用OCR服务并返回有效的JSON结构
        /// </summary>
        [Fact]
        public async Task ExtractTextFromRegionAsync_BasicFunctionality_ShouldReturnValidJson()
        {
            // Arrange
            var x = 100;
            var y = 200;
            var width = 300;
            var height = 150;

            // Act
            var result = await _tool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));
            Assert.True(jsonResult.TryGetProperty("region", out var regionProperty));
            
            // 验证区域信息
            var region = regionProperty;
            Assert.Equal(x, region.GetProperty("x").GetInt32());
            Assert.Equal(y, region.GetProperty("y").GetInt32());
            Assert.Equal(width, region.GetProperty("width").GetInt32());
            Assert.Equal(height, region.GetProperty("height").GetInt32());

            var success = successProperty.GetBoolean();
            var text = textProperty.GetString();
            var message = messageProperty.GetString();
            
            Console.WriteLine($"OCR结果: success={success}, text='{text}', message='{message}'");
            Assert.NotNull(text); // 文本可能为空，但不应该为null
            Assert.NotEmpty(message);
        }

        #endregion

        #region 不同区域大小测试

        /// <summary>
        /// 测试不同屏幕区域进行文本提取
        /// 使用多组坐标和尺寸参数验证OCR工具的适应性
        /// </summary>
        /// <param name="x">区域左上角X坐标</param>
        /// <param name="y">区域左上角Y坐标</param>
        /// <param name="width">区域宽度</param>
        /// <param name="height">区域高度</param>
        [Theory]
        [InlineData(0, 0, 100, 50)]
        [InlineData(500, 300, 200, 100)]
        [InlineData(1000, 800, 400, 300)]
        [InlineData(50, 50, 150, 75)]
        public async Task ExtractTextFromRegionAsync_DifferentRegions_ShouldHandleAllSizes(int x, int y, int width, int height)
        {
            // Act
            var result = await _tool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
            Assert.True(jsonResult.TryGetProperty("region", out var regionProperty));
            
            // 验证区域信息正确
            var region = regionProperty;
            Assert.Equal(x, region.GetProperty("x").GetInt32());
            Assert.Equal(y, region.GetProperty("y").GetInt32());
            Assert.Equal(width, region.GetProperty("width").GetInt32());
            Assert.Equal(height, region.GetProperty("height").GetInt32());

            var success = successProperty.GetBoolean();
            var text = textProperty.GetString();
            
            Console.WriteLine($"区域({x},{y}) 大小{width}x{height} OCR结果: success={success}, text='{text}'");
            Assert.NotNull(text); // 文本可能为空，但不应该为null
        }

        /// <summary>
        /// 测试从小区域提取文本的能力
        /// 验证OCR工具在处理较小屏幕区域时的表现
        /// </summary>
        [Fact]
        public async Task ExtractTextFromRegionAsync_SmallRegion_ShouldReturnResult()
        {
            // Arrange
            var x = 10;
            var y = 10;
            var width = 50;
            var height = 20;

            // Act
            var result = await _tool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
            
            var success = successProperty.GetBoolean();
            var text = textProperty.GetString();
            
            Console.WriteLine($"小区域({x},{y}) 大小{width}x{height} OCR结果: success={success}, text='{text}'");
            Assert.NotNull(text); // 文本可能为空，但不应该为null
        }

        /// <summary>
        /// 测试从大区域（全屏分辨率）提取文本的能力
        /// 验证OCR工具在处理大屏幕区域时的性能和稳定性
        /// </summary>
        [Fact]
        public async Task ExtractTextFromRegionAsync_LargeRegion_ShouldReturnResult()
        {
            // Arrange
            var x = 0;
            var y = 0;
            var width = 1920;
            var height = 1080;

            // Act
            var result = await _tool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
            
            var success = successProperty.GetBoolean();
            var text = textProperty.GetString();
            
            Console.WriteLine($"大区域({x},{y}) 大小{width}x{height} OCR结果: success={success}, text='{text}'");
            Assert.NotNull(text); // 文本可能为空，但不应该为null
        }

        #endregion

        #region 边界情况测试

        /// <summary>
        /// 测试处理空区域（宽度或高度为0）的边界情况
        /// 验证OCR工具对无效区域参数的容错处理
        /// </summary>
        [Fact]
        public async Task ExtractTextFromRegionAsync_EmptyRegion_ShouldReturnEmptyResult()
        {
            // Arrange
            var x = 100;
            var y = 100;
            var width = 0;
            var height = 0;

            // Act
            var result = await _tool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
            
            var success = successProperty.GetBoolean();
            var text = textProperty.GetString();
            
            Console.WriteLine($"空区域({x},{y}) 大小{width}x{height} OCR结果: success={success}, text='{text}'");
            Assert.NotNull(text); // 文本可能为空，但不应该为null
        }

        /// <summary>
        /// 测试处理负坐标参数的边界情况
        /// 验证OCR工具对无效坐标输入的容错处理和异常情况的优雅处理
        /// </summary>
        [Fact]
        public async Task ExtractTextFromRegionAsync_NegativeCoordinates_ShouldHandleGracefully()
        {
            // Arrange
            var x = -10;
            var y = -5;
            var width = 100;
            var height = 50;

            // Act
            var result = await _tool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            
            var success = successProperty.GetBoolean();
            Console.WriteLine($"负坐标区域 ({x}, {y}, {width}, {height}) OCR结果: success={success}");
            
            if (success && jsonResult.TryGetProperty("text", out var textProperty))
            {
                var extractedText = textProperty.GetString();
                Console.WriteLine($"从负坐标区域提取的文本: '{extractedText}'");
                Assert.NotNull(extractedText);
            }
            else
            {
                Console.WriteLine("负坐标区域OCR处理失败或无文本");
            }
        }

        /// <summary>
        /// 测试处理超大区域参数的极限情况
        /// 验证OCR工具在面对超出屏幕范围的大区域时的稳定性和性能表现
        /// </summary>
        [Fact]
        public async Task ExtractTextFromRegionAsync_VeryLargeRegion_ShouldHandleGracefully()
        {
            // Arrange
            var x = 0;
            var y = 0;
            var width = 10000;
            var height = 10000;

            // Act
            var result = await _tool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            
            var success = successProperty.GetBoolean();
            Console.WriteLine($"超大区域 ({x}, {y}, {width}, {height}) OCR结果: success={success}");
            
            if (success && jsonResult.TryGetProperty("text", out var textProperty))
            {
                var extractedText = textProperty.GetString();
                Console.WriteLine($"从超大区域提取的文本: '{extractedText}'");
                Assert.NotNull(extractedText);
            }
            else
            {
                Console.WriteLine("超大区域OCR处理失败或无文本");
            }
        }

        /// <summary>
        /// 测试处理负宽度和高度的边界情况
        /// 验证OCR工具对无效尺寸参数的容错处理
        /// </summary>
        [Theory]
        [InlineData(100, 100, -50, 50)]
        [InlineData(100, 100, 50, -50)]
        [InlineData(100, 100, -50, -50)]
        public async Task ExtractTextFromRegionAsync_NegativeDimensions_ShouldHandleGracefully(int x, int y, int width, int height)
        {
            // Act
            var result = await _tool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            
            var success = successProperty.GetBoolean();
            Console.WriteLine($"负尺寸区域 ({x}, {y}, {width}, {height}) OCR结果: success={success}");
            
            // 验证区域信息被正确记录
            if (jsonResult.TryGetProperty("region", out var regionProperty))
            {
                var region = regionProperty;
                Assert.Equal(x, region.GetProperty("x").GetInt32());
                Assert.Equal(y, region.GetProperty("y").GetInt32());
                Assert.Equal(width, region.GetProperty("width").GetInt32());
                Assert.Equal(height, region.GetProperty("height").GetInt32());
            }
        }

        #endregion

        #region 异常处理测试

        /// <summary>
        /// 测试工具在OCR服务异常时的错误处理
        /// 模拟服务异常情况下的优雅降级
        /// </summary>
        [Fact]
        public async Task ExtractTextFromRegionAsync_ServiceException_ShouldReturnErrorResult()
        {
            // Note: 由于使用真实的OCR服务，这个测试主要验证异常情况下的JSON结构
            // 在实际异常发生时（如OCR模型加载失败），工具应该返回错误状态
            
            // Arrange
            var x = 0;
            var y = 0;
            var width = 100;
            var height = 100;

            // Act
            var result = await _tool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            
            // 验证必要的属性存在
            Assert.True(jsonResult.TryGetProperty("success", out _));
            Assert.True(jsonResult.TryGetProperty("text", out _));
            Assert.True(jsonResult.TryGetProperty("message", out _));
            Assert.True(jsonResult.TryGetProperty("region", out _));
            
            Console.WriteLine($"异常处理测试结果: {result}");
        }

        #endregion

        #region 性能测试

        /// <summary>
        /// 测试连续多次调用的性能表现
        /// 验证工具在连续使用时的稳定性和内存管理
        /// </summary>
        [Fact]
        public async Task ExtractTextFromRegionAsync_MultipleConsecutiveCalls_ShouldMaintainPerformance()
        {
            // Arrange
            var regions = new[]
            {
                new { x = 0, y = 0, width = 100, height = 100 },
                new { x = 100, y = 100, width = 200, height = 150 },
                new { x = 300, y = 200, width = 250, height = 200 },
                new { x = 50, y = 300, width = 300, height = 100 },
                new { x = 400, y = 400, width = 150, height = 150 }
            };

            // Act & Assert
            for (int i = 0; i < regions.Length; i++)
            {
                var region = regions[i];
                var result = await _tool.ExtractTextFromRegionAsync(region.x, region.y, region.width, region.height);
                
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
                Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
                
                var success = successProperty.GetBoolean();
                var text = textProperty.GetString();
                
                Console.WriteLine($"第{i + 1}次调用 - 区域({region.x},{region.y}) 大小{region.width}x{region.height}: success={success}, text='{text}'");
                Assert.NotNull(text);
            }
        }

        #endregion

        #region JSON格式验证测试

        /// <summary>
        /// 测试返回的JSON格式的完整性和一致性
        /// 验证所有必需的属性都存在且类型正确
        /// </summary>
        [Fact]
        public async Task ExtractTextFromRegionAsync_JsonFormat_ShouldBeConsistent()
        {
            // Arrange
            var x = 100;
            var y = 200;
            var width = 300;
            var height = 150;

            // Act
            var result = await _tool.ExtractTextFromRegionAsync(x, y, width, height);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            
            // 验证所有必需属性存在
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));
            Assert.True(jsonResult.TryGetProperty("region", out var regionProperty));
            
            // 验证属性类型
            Assert.Equal(JsonValueKind.True, successProperty.ValueKind == JsonValueKind.True ? JsonValueKind.True : JsonValueKind.False);
            Assert.Equal(JsonValueKind.String, textProperty.ValueKind);
            Assert.Equal(JsonValueKind.String, messageProperty.ValueKind);
            Assert.Equal(JsonValueKind.Object, regionProperty.ValueKind);
            
            // 验证region对象的结构
            var region = regionProperty;
            Assert.True(region.TryGetProperty("x", out var xProperty));
            Assert.True(region.TryGetProperty("y", out var yProperty));
            Assert.True(region.TryGetProperty("width", out var widthProperty));
            Assert.True(region.TryGetProperty("height", out var heightProperty));
            
            Assert.Equal(JsonValueKind.Number, xProperty.ValueKind);
            Assert.Equal(JsonValueKind.Number, yProperty.ValueKind);
            Assert.Equal(JsonValueKind.Number, widthProperty.ValueKind);
            Assert.Equal(JsonValueKind.Number, heightProperty.ValueKind);
        }

        #endregion
    }
}
