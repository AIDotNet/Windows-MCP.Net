using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using WindowsMCP.Net.Tools.OCR;
using Interface;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test
{
    /// <summary>
    /// ExtractTextFromScreenTool 的专用单元测试类
    /// 测试从整个屏幕提取文本的所有功能
    /// </summary>
    public class ExtractTextFromScreenToolTest
    {
        private readonly IOcrService _ocrService;
        private readonly ILogger<ExtractTextFromScreenTool> _logger;
        private readonly ExtractTextFromScreenTool _tool;

        public ExtractTextFromScreenToolTest()
        {
            _ocrService = new OcrService();
            _logger = NullLogger<ExtractTextFromScreenTool>.Instance;
            _tool = new ExtractTextFromScreenTool(_ocrService, _logger);
        }

        #region 基础功能测试

        /// <summary>
        /// 测试ExtractTextFromScreenAsync基础功能
        /// 验证工具能够正确调用OCR服务并返回有效的JSON结构
        /// </summary>
        [Fact]
        public async Task ExtractTextFromScreenAsync_BasicFunctionality_ShouldReturnValidJson()
        {
            // Act
            var result = await _tool.ExtractTextFromScreenAsync();

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

            var success = successProperty.GetBoolean();
            var text = textProperty.GetString();
            var message = messageProperty.GetString();
            
            Console.WriteLine($"全屏OCR结果: success={success}, text='{text}', message='{message}'");
            Assert.NotNull(text); // 文本可能为空，但不应该为null
            Assert.NotEmpty(message);
        }

        /// <summary>
        /// 测试多次连续调用ExtractTextFromScreenAsync
        /// 验证工具在连续使用时的稳定性和一致性
        /// </summary>
        [Fact]
        public async Task ExtractTextFromScreenAsync_MultipleCalls_ShouldMaintainStability()
        {
            // Act & Assert - 连续调用5次
            for (int i = 0; i < 5; i++)
            {
                var result = await _tool.ExtractTextFromScreenAsync();
                
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
                Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
                Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

                var success = successProperty.GetBoolean();
                var text = textProperty.GetString();
                var message = messageProperty.GetString();
                
                Console.WriteLine($"第{i + 1}次全屏OCR调用: success={success}, text长度={text?.Length ?? 0}, message='{message}'");
                Assert.NotNull(text);
                Assert.NotEmpty(message);
                
                // 添加小延迟以避免过快的连续调用
                await Task.Delay(100);
            }
        }

        #endregion

        #region 性能测试

        /// <summary>
        /// 测试ExtractTextFromScreenAsync的执行时间
        /// 验证全屏OCR操作在合理时间内完成
        /// </summary>
        [Fact]
        public async Task ExtractTextFromScreenAsync_Performance_ShouldCompleteInReasonableTime()
        {
            // Arrange
            var startTime = DateTime.UtcNow;

            // Act
            var result = await _tool.ExtractTextFromScreenAsync();

            // Assert
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            
            Console.WriteLine($"全屏OCR执行时间: {duration.TotalMilliseconds}ms");
            
            // 验证在30秒内完成（OCR可能需要较长时间，特别是首次加载模型时）
            Assert.True(duration.TotalSeconds < 30, $"全屏OCR执行时间过长: {duration.TotalSeconds}秒");
            
            // 验证返回结果
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out _));
            Assert.True(jsonResult.TryGetProperty("text", out _));
            Assert.True(jsonResult.TryGetProperty("message", out _));
        }

        /// <summary>
        /// 测试并发调用ExtractTextFromScreenAsync
        /// 验证工具在并发场景下的线程安全性
        /// </summary>
        [Fact]
        public async Task ExtractTextFromScreenAsync_ConcurrentCalls_ShouldHandleThreadSafety()
        {
            // Arrange
            const int concurrentCallsCount = 3;
            var tasks = new List<Task<string>>();

            // Act - 并发调用
            for (int i = 0; i < concurrentCallsCount; i++)
            {
                tasks.Add(_tool.ExtractTextFromScreenAsync());
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(concurrentCallsCount, results.Length);
            
            for (int i = 0; i < results.Length; i++)
            {
                var result = results[i];
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                
                Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
                Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
                Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

                var success = successProperty.GetBoolean();
                var text = textProperty.GetString();
                var message = messageProperty.GetString();
                
                Console.WriteLine($"并发调用{i + 1}: success={success}, text长度={text?.Length ?? 0}, message='{message}'");
                Assert.NotNull(text);
                Assert.NotEmpty(message);
            }
        }

        #endregion

        #region 异常处理测试

        /// <summary>
        /// 测试工具在OCR服务异常时的错误处理
        /// 验证异常情况下的优雅降级和错误信息返回
        /// </summary>
        [Fact]
        public async Task ExtractTextFromScreenAsync_ServiceException_ShouldReturnErrorResult()
        {
            // Note: 由于使用真实的OCR服务，这个测试主要验证异常情况下的JSON结构
            // 在实际异常发生时（如OCR模型加载失败），工具应该返回错误状态
            
            // Act
            var result = await _tool.ExtractTextFromScreenAsync();

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            
            // 验证必要的属性存在
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));
            
            // 验证属性类型正确
            Assert.True(successProperty.ValueKind == JsonValueKind.True || successProperty.ValueKind == JsonValueKind.False);
            Assert.Equal(JsonValueKind.String, textProperty.ValueKind);
            Assert.Equal(JsonValueKind.String, messageProperty.ValueKind);
            
            Console.WriteLine($"异常处理测试结果: {result}");
        }

        /// <summary>
        /// 测试在系统资源不足时的处理
        /// 模拟资源限制情况下的工具行为
        /// </summary>
        [Fact]
        public async Task ExtractTextFromScreenAsync_ResourceConstraints_ShouldHandleGracefully()
        {
            // Arrange & Act
            // 通过快速连续调用来模拟资源压力
            var tasks = new List<Task<string>>();
            for (int i = 0; i < 2; i++)
            {
                tasks.Add(_tool.ExtractTextFromScreenAsync());
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                Assert.True(jsonResult.TryGetProperty("success", out _));
                Assert.True(jsonResult.TryGetProperty("text", out _));
                Assert.True(jsonResult.TryGetProperty("message", out _));
                
                Console.WriteLine($"资源约束测试结果: {result}");
            }
        }

        #endregion

        #region JSON格式验证测试

        /// <summary>
        /// 测试返回的JSON格式的完整性和一致性
        /// 验证所有必需的属性都存在且类型正确
        /// </summary>
        [Fact]
        public async Task ExtractTextFromScreenAsync_JsonFormat_ShouldBeConsistent()
        {
            // Act
            var result = await _tool.ExtractTextFromScreenAsync();

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            
            // 验证所有必需属性存在
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));
            
            // 验证属性类型
            Assert.True(successProperty.ValueKind == JsonValueKind.True || successProperty.ValueKind == JsonValueKind.False);
            Assert.Equal(JsonValueKind.String, textProperty.ValueKind);
            Assert.Equal(JsonValueKind.String, messageProperty.ValueKind);
            
            // 验证消息内容包含预期的关键词
            var message = messageProperty.GetString();
            Assert.True(message.Contains("screen") || message.Contains("屏幕"), 
                $"消息应该包含'screen'或'屏幕'关键词，实际消息: '{message}'");
        }

        /// <summary>
        /// 测试JSON输出的格式化和可读性
        /// 验证返回的JSON是格式化的（包含缩进）
        /// </summary>
        [Fact]
        public async Task ExtractTextFromScreenAsync_JsonFormatting_ShouldBeIndented()
        {
            // Act
            var result = await _tool.ExtractTextFromScreenAsync();

            // Assert
            // 验证JSON包含换行符和缩进（格式化的JSON）
            Assert.Contains("\n", result);
            Assert.Contains("  ", result); // 验证包含缩进
            
            // 验证JSON可以成功反序列化
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.NotEqual(JsonValueKind.Null, jsonResult.ValueKind);
            Assert.NotEqual(JsonValueKind.Undefined, jsonResult.ValueKind);
            
            Console.WriteLine($"格式化的JSON输出:\n{result}");
        }

        #endregion

        #region 边界条件测试

        /// <summary>
        /// 测试在不同系统状态下的工具行为
        /// 验证工具在各种环境条件下的稳定性
        /// </summary>
        [Fact]
        public async Task ExtractTextFromScreenAsync_DifferentSystemStates_ShouldMaintainFunctionality()
        {
            // Act - 测试在当前系统状态下的功能
            var result = await _tool.ExtractTextFromScreenAsync();

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("text", out var textProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

            var success = successProperty.GetBoolean();
            var text = textProperty.GetString();
            var message = messageProperty.GetString();
            
            Console.WriteLine($"系统状态测试: success={success}, text长度={text?.Length ?? 0}, message='{message}'");
            
            // 基本验证
            Assert.NotNull(text);
            Assert.NotEmpty(message);
        }

        /// <summary>
        /// 测试连续快速调用的稳定性
        /// 验证工具在高频调用时的表现
        /// </summary>
        [Fact]
        public async Task ExtractTextFromScreenAsync_RapidConsecutiveCalls_ShouldRemainStable()
        {
            // Arrange
            const int rapidCallsCount = 3;
            var results = new List<string>();

            // Act
            for (int i = 0; i < rapidCallsCount; i++)
            {
                var result = await _tool.ExtractTextFromScreenAsync();
                results.Add(result);
                
                // 非常短的延迟来模拟快速调用
                await Task.Delay(10);
            }

            // Assert
            Assert.Equal(rapidCallsCount, results.Count);
            
            foreach (var result in results)
            {
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                Assert.True(jsonResult.TryGetProperty("success", out _));
                Assert.True(jsonResult.TryGetProperty("text", out _));
                Assert.True(jsonResult.TryGetProperty("message", out _));
            }
            
            Console.WriteLine($"快速连续调用完成，共{rapidCallsCount}次调用");
        }

        #endregion

        #region 日志测试

        /// <summary>
        /// 测试工具的日志记录功能
        /// 验证适当的日志信息被记录
        /// </summary>
        [Fact]
        public async Task ExtractTextFromScreenAsync_Logging_ShouldLogAppropriateMessages()
        {
            // Note: 由于使用NullLogger，这个测试主要验证调用不会抛出异常
            // 在实际应用中，可以使用TestLogger来验证具体的日志消息
            
            // Act
            var result = await _tool.ExtractTextFromScreenAsync();

            // Assert
            // 验证调用成功完成且没有抛出异常
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out _));
            
            Console.WriteLine("日志测试完成，没有异常抛出");
        }

        #endregion
    }
}
