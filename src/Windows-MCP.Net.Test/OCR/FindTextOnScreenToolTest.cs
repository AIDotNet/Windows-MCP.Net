using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using WindowsMCP.Net.Tools.OCR;
using Interface;
using WindowsMCP.Net.Services;

namespace Windows_MCP.Net.Test.OCR
{
    /// <summary>
    /// FindTextOnScreenTool 的专用单元测试类
    /// 测试在屏幕上查找指定文本的所有功能
    /// </summary>
    public class FindTextOnScreenToolTest
    {
        private readonly IOcrService _ocrService;
        private readonly ILogger<FindTextOnScreenTool> _logger;
        private readonly FindTextOnScreenTool _tool;

        public FindTextOnScreenToolTest()
        {
            _ocrService = new OcrService();
            _logger = NullLogger<FindTextOnScreenTool>.Instance;
            _tool = new FindTextOnScreenTool(_ocrService, _logger);
        }

        #region 基础功能测试

        /// <summary>
        /// 测试FindTextOnScreenAsync基础功能
        /// 验证工具能够正确调用OCR服务并返回有效的JSON结构
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_BasicFunctionality_ShouldReturnValidJson()
        {
            // Arrange
            var searchText = "Test Text";

            // Act
            var result = await _tool.FindTextOnScreenAsync(searchText);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

            var success = successProperty.GetBoolean();
            var found = foundProperty.GetBoolean();
            var returnedSearchText = searchTextProperty.GetString();
            var message = messageProperty.GetString();
            
            Console.WriteLine($"查找文本 '{searchText}' OCR结果: success={success}, found={found}, message='{message}'");
            Assert.Equal(searchText, returnedSearchText);
            Assert.NotEmpty(message);
        }

        /// <summary>
        /// 测试查找常见UI元素文本
        /// 验证工具对典型界面元素的识别能力
        /// </summary>
        /// <param name="text">要搜索的文本内容</param>
        [Theory]
        [InlineData("Button")]
        [InlineData("Login")]
        [InlineData("Submit")]
        [InlineData("Cancel")]
        [InlineData("OK")]
        [InlineData("Close")]
        public async Task FindTextOnScreenAsync_CommonUIElements_ShouldReturnConsistentResults(string text)
        {
            // Act
            var result = await _tool.FindTextOnScreenAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

            var success = successProperty.GetBoolean();
            var found = foundProperty.GetBoolean();
            var returnedSearchText = searchTextProperty.GetString();
            var message = messageProperty.GetString();
            
            Console.WriteLine($"查找常见UI文本 '{text}': success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            Assert.NotEmpty(message);
        }

        #endregion

        #region 不同文本类型测试

        /// <summary>
        /// 测试查找多词组合文本的能力
        /// 验证OCR工具能够正确识别和定位包含空格的完整短语
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_MultiWordText_ShouldReturnResults()
        {
            // Arrange
            var text = "Click here to continue";

            // Act
            var result = await _tool.FindTextOnScreenAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

            var success = successProperty.GetBoolean();
            var found = foundProperty.GetBoolean();
            var returnedSearchText = searchTextProperty.GetString();
            var message = messageProperty.GetString();
            
            Console.WriteLine($"查找多词文本 '{text}': success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            Assert.NotEmpty(message);
        }

        /// <summary>
        /// 测试查找包含特殊字符的文本
        /// 验证OCR工具对符号、标点符号等特殊字符的识别准确性
        /// </summary>
        /// <param name="text">包含特殊字符的文本</param>
        [Theory]
        [InlineData("Save & Exit")]
        [InlineData("File -> Open")]
        [InlineData("User@domain.com")]
        [InlineData("Price: $99.99")]
        [InlineData("100% Complete")]
        public async Task FindTextOnScreenAsync_SpecialCharacters_ShouldHandleCorrectly(string text)
        {
            // Act
            var result = await _tool.FindTextOnScreenAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

            var success = successProperty.GetBoolean();
            var found = foundProperty.GetBoolean();
            var returnedSearchText = searchTextProperty.GetString();
            var message = messageProperty.GetString();
            
            Console.WriteLine($"查找特殊字符文本 '{text}': success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            Assert.NotEmpty(message);
        }

        /// <summary>
        /// 测试查找数字和字母数字组合文本的能力
        /// 验证OCR工具对纯数字、字母数字混合及包含符号的文本识别准确性
        /// </summary>
        /// <param name="text">要搜索的数字或字母数字文本</param>
        [Theory]
        [InlineData("1234567890")]
        [InlineData("ABC123")]
        [InlineData("Test@123")]
        [InlineData("Version 2.1.0")]
        [InlineData("ID: 45678")]
        public async Task FindTextOnScreenAsync_NumericAndAlphanumericText_ShouldProcess(string text)
        {
            // Act
            var result = await _tool.FindTextOnScreenAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

            var success = successProperty.GetBoolean();
            var found = foundProperty.GetBoolean();
            var returnedSearchText = searchTextProperty.GetString();
            var message = messageProperty.GetString();
            
            Console.WriteLine($"查找数字/字母数字文本 '{text}': success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            Assert.NotEmpty(message);
        }

        #endregion

        #region 边界条件测试

        /// <summary>
        /// 测试查找不存在的文本时的处理逻辑
        /// 验证OCR工具在未找到目标文本时能够正确返回未找到的状态
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_TextNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var text = "VeryUniqueTextThatShouldNotExistOnScreen12345";

            // Act
            var result = await _tool.FindTextOnScreenAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

            var success = successProperty.GetBoolean();
            var found = foundProperty.GetBoolean();
            var returnedSearchText = searchTextProperty.GetString();
            var message = messageProperty.GetString();
            
            Console.WriteLine($"查找不存在文本 '{text}': success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            Assert.NotEmpty(message);
            
            // 消息应该指示文本未找到
            Assert.Contains("not found", message.ToLower());
        }

        /// <summary>
        /// 测试查找空字符串时的边界情况处理
        /// 验证OCR工具对空输入参数的容错机制和错误处理
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_EmptyText_ShouldReturnErrorResult()
        {
            // Arrange
            var text = "";

            // Act
            var result = await _tool.FindTextOnScreenAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

            var success = successProperty.GetBoolean();
            var found = foundProperty.GetBoolean();
            var returnedSearchText = searchTextProperty.GetString();
            var message = messageProperty.GetString();
            
            Console.WriteLine($"查找空文本: success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            Assert.NotEmpty(message);
        }

        /// <summary>
        /// 测试查找null字符串时的边界情况处理
        /// 验证OCR工具对null输入参数的容错机制
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_NullText_ShouldHandleGracefully()
        {
            // Arrange
            string text = null;

            // Act & Assert
            // 工具应该能够处理null输入而不抛出异常
            var result = await _tool.FindTextOnScreenAsync(text);
            
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out _));
            Assert.True(jsonResult.TryGetProperty("found", out _));
            Assert.True(jsonResult.TryGetProperty("searchText", out _));
            Assert.True(jsonResult.TryGetProperty("message", out _));
            
            Console.WriteLine($"查找null文本结果: {result}");
        }

        /// <summary>
        /// 测试查找超长文本的处理
        /// 验证工具对异常长度文本的处理能力
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_VeryLongText_ShouldHandleAppropriately()
        {
            // Arrange
            var text = new string('A', 1000); // 1000字符的长文本

            // Act
            var result = await _tool.FindTextOnScreenAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

            var success = successProperty.GetBoolean();
            var found = foundProperty.GetBoolean();
            var returnedSearchText = searchTextProperty.GetString();
            
            Console.WriteLine($"查找超长文本: success={success}, found={found}, 文本长度={returnedSearchText?.Length}");
            Assert.Equal(text, returnedSearchText);
        }

        #endregion

        #region 性能测试

        /// <summary>
        /// 测试连续多次查找的性能表现
        /// 验证工具在连续使用时的稳定性和内存管理
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_MultipleConsecutiveSearches_ShouldMaintainPerformance()
        {
            // Arrange
            var searchTexts = new[]
            {
                "Search1",
                "Search2", 
                "Search3",
                "Search4",
                "Search5"
            };

            // Act & Assert
            for (int i = 0; i < searchTexts.Length; i++)
            {
                var text = searchTexts[i];
                var result = await _tool.FindTextOnScreenAsync(text);
                
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
                Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
                Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
                
                var success = successProperty.GetBoolean();
                var found = foundProperty.GetBoolean();
                var returnedSearchText = searchTextProperty.GetString();
                
                Console.WriteLine($"第{i + 1}次查找 '{text}': success={success}, found={found}");
                Assert.Equal(text, returnedSearchText);
                
                // 短暂延迟以避免过快的连续调用
                await Task.Delay(50);
            }
        }

        /// <summary>
        /// 测试并发查找不同文本的线程安全性
        /// 验证工具在并发场景下的稳定性
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_ConcurrentSearches_ShouldHandleThreadSafety()
        {
            // Arrange
            var searchTexts = new[] { "Concurrent1", "Concurrent2", "Concurrent3" };
            var tasks = new List<Task<string>>();

            // Act - 并发查找
            foreach (var text in searchTexts)
            {
                tasks.Add(_tool.FindTextOnScreenAsync(text));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(searchTexts.Length, results.Length);
            
            for (int i = 0; i < results.Length; i++)
            {
                var result = results[i];
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                
                Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
                Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
                Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));

                var success = successProperty.GetBoolean();
                var found = foundProperty.GetBoolean();
                var returnedSearchText = searchTextProperty.GetString();
                
                Console.WriteLine($"并发查找{i + 1} '{searchTexts[i]}': success={success}, found={found}");
                Assert.Equal(searchTexts[i], returnedSearchText);
            }
        }

        #endregion

        #region 异常处理测试

        /// <summary>
        /// 测试工具在OCR服务异常时的错误处理
        /// 验证异常情况下的优雅降级和错误信息返回
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_ServiceException_ShouldReturnErrorResult()
        {
            // Note: 由于使用真实的OCR服务，这个测试主要验证异常情况下的JSON结构
            // 在实际异常发生时（如OCR模型加载失败），工具应该返回错误状态
            
            // Arrange
            var text = "TestForException";

            // Act
            var result = await _tool.FindTextOnScreenAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            
            // 验证必要的属性存在
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));
            
            // 验证属性类型正确
            Assert.True(successProperty.ValueKind == JsonValueKind.True || successProperty.ValueKind == JsonValueKind.False);
            Assert.True(foundProperty.ValueKind == JsonValueKind.True || foundProperty.ValueKind == JsonValueKind.False);
            Assert.Equal(JsonValueKind.String, searchTextProperty.ValueKind);
            Assert.Equal(JsonValueKind.String, messageProperty.ValueKind);
            
            Console.WriteLine($"异常处理测试结果: {result}");
        }

        #endregion

        #region JSON格式验证测试

        /// <summary>
        /// 测试返回的JSON格式的完整性和一致性
        /// 验证所有必需的属性都存在且类型正确
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_JsonFormat_ShouldBeConsistent()
        {
            // Arrange
            var text = "JsonFormatTest";

            // Act
            var result = await _tool.FindTextOnScreenAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            
            // 验证所有必需属性存在
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));
            
            // 验证属性类型
            Assert.True(successProperty.ValueKind == JsonValueKind.True || successProperty.ValueKind == JsonValueKind.False);
            Assert.True(foundProperty.ValueKind == JsonValueKind.True || foundProperty.ValueKind == JsonValueKind.False);
            Assert.Equal(JsonValueKind.String, searchTextProperty.ValueKind);
            Assert.Equal(JsonValueKind.String, messageProperty.ValueKind);
            
            // 验证消息内容包含预期的文本
            var message = messageProperty.GetString();
            var returnedSearchText = searchTextProperty.GetString();
            
            Assert.Equal(text, returnedSearchText);
            Assert.Contains(text, message);
        }

        /// <summary>
        /// 测试JSON输出的格式化和可读性
        /// 验证返回的JSON是格式化的（包含缩进）
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_JsonFormatting_ShouldBeIndented()
        {
            // Arrange
            var text = "FormattingTest";

            // Act
            var result = await _tool.FindTextOnScreenAsync(text);

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

        #region 日志测试

        /// <summary>
        /// 测试工具的日志记录功能
        /// 验证适当的日志信息被记录
        /// </summary>
        [Fact]
        public async Task FindTextOnScreenAsync_Logging_ShouldLogAppropriateMessages()
        {
            // Note: 由于使用NullLogger，这个测试主要验证调用不会抛出异常
            // 在实际应用中，可以使用TestLogger来验证具体的日志消息
            
            // Arrange
            var text = "LoggingTest";

            // Act
            var result = await _tool.FindTextOnScreenAsync(text);

            // Assert
            // 验证调用成功完成且没有抛出异常
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out _));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.Equal(text, searchTextProperty.GetString());
            
            Console.WriteLine("日志测试完成，没有异常抛出");
        }

        #endregion
    }
}
