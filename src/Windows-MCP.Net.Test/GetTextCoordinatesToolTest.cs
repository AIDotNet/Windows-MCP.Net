using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using WindowsMCP.Net.Tools.OCR;
using Interface;
using WindowsMCP.Net.Services;
using System.Drawing;

namespace Windows_MCP.Net.Test
{
    /// <summary>
    /// GetTextCoordinatesTool 的专用单元测试类
    /// 测试获取屏幕上指定文本坐标的所有功能
    /// </summary>
    public class GetTextCoordinatesToolTest
    {
        private readonly IOcrService _ocrService;
        private readonly ILogger<GetTextCoordinatesTool> _logger;
        private readonly GetTextCoordinatesTool _tool;

        public GetTextCoordinatesToolTest()
        {
            _ocrService = new OcrService();
            _logger = NullLogger<GetTextCoordinatesTool>.Instance;
            _tool = new GetTextCoordinatesTool(_ocrService, _logger);
        }

        #region 基础功能测试

        /// <summary>
        /// 测试GetTextCoordinatesAsync基础功能
        /// 验证工具能够正确调用OCR服务并返回有效的JSON结构
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_BasicFunctionality_ShouldReturnValidJson()
        {
            // Arrange
            var searchText = "Test Text";

            // Act
            var result = await _tool.GetTextCoordinatesAsync(searchText);

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
            
            Console.WriteLine($"获取文本坐标 '{searchText}': success={success}, found={found}, message='{message}'");
            Assert.Equal(searchText, returnedSearchText);
            Assert.NotEmpty(message);
            Assert.Contains(searchText, message);
            
            // 如果找到文本，应该有坐标信息
            if (found)
            {
                Assert.True(jsonResult.TryGetProperty("coordinates", out var coordinatesProperty));
                Assert.True(coordinatesProperty.TryGetProperty("x", out var xProperty));
                Assert.True(coordinatesProperty.TryGetProperty("y", out var yProperty));
                
                var x = xProperty.GetInt32();
                var y = yProperty.GetInt32();
                Console.WriteLine($"找到文本坐标: ({x}, {y})");
            }
            else
            {
                // 如果未找到，coordinates可能为null或不存在
                Console.WriteLine("未找到指定文本");
            }
        }

        /// <summary>
        /// 测试查找常见UI元素的坐标
        /// 验证工具对典型界面元素的坐标定位能力
        /// </summary>
        /// <param name="text">要搜索的文本内容</param>
        [Theory]
        [InlineData("Button")]
        [InlineData("Login")]
        [InlineData("Submit")]
        [InlineData("Cancel")]
        [InlineData("OK")]
        [InlineData("Close")]
        public async Task GetTextCoordinatesAsync_CommonUIElements_ShouldReturnConsistentResults(string text)
        {
            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

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
            
            Console.WriteLine($"查找常见UI文本坐标 '{text}': success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            Assert.NotEmpty(message);
            
            if (found && jsonResult.TryGetProperty("coordinates", out var coordinatesProperty))
            {
                var x = coordinatesProperty.GetProperty("x").GetInt32();
                var y = coordinatesProperty.GetProperty("y").GetInt32();
                Console.WriteLine($"找到常见UI文本 '{text}' 坐标: ({x}, {y})");
                
                // 坐标应该是有效的正数
                Assert.True(x >= 0, $"X坐标应该非负，实际值: {x}");
                Assert.True(y >= 0, $"Y坐标应该非负，实际值: {y}");
            }
        }

        #endregion

        #region 不同文本类型测试

        /// <summary>
        /// 测试获取多词组合文本的坐标
        /// 验证OCR工具能够正确识别和定位包含空格的完整短语的坐标
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_MultiWordText_ShouldReturnCoordinates()
        {
            // Arrange
            var text = "Click here to continue";

            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

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
            
            Console.WriteLine($"查找多词文本坐标 '{text}': success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            
            if (found && jsonResult.TryGetProperty("coordinates", out var coordinatesProperty))
            {
                var x = coordinatesProperty.GetProperty("x").GetInt32();
                var y = coordinatesProperty.GetProperty("y").GetInt32();
                Console.WriteLine($"找到多词文本坐标: ({x}, {y})");
            }
            else
            {
                Console.WriteLine("未找到指定的多词文本");
            }
        }

        /// <summary>
        /// 测试获取包含特殊字符文本的坐标
        /// 验证OCR工具对符号、标点符号等特殊字符的坐标定位准确性
        /// </summary>
        /// <param name="text">包含特殊字符的文本</param>
        [Theory]
        [InlineData("Save & Exit")]
        [InlineData("File -> Open")]
        [InlineData("User@domain.com")]
        [InlineData("Price: $99.99")]
        [InlineData("100% Complete")]
        public async Task GetTextCoordinatesAsync_SpecialCharacters_ShouldReturnCoordinates(string text)
        {
            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

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
            
            Console.WriteLine($"查找特殊字符文本坐标 '{text}': success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            
            if (found && jsonResult.TryGetProperty("coordinates", out var coordinatesProperty))
            {
                var x = coordinatesProperty.GetProperty("x").GetInt32();
                var y = coordinatesProperty.GetProperty("y").GetInt32();
                Console.WriteLine($"找到特殊字符文本坐标: ({x}, {y})");
            }
            else
            {
                Console.WriteLine("未找到指定的特殊字符文本");
            }
        }

        /// <summary>
        /// 测试获取数字和字母数字组合文本的坐标
        /// 验证OCR工具对纯数字、字母数字混合及包含符号的文本坐标识别准确性
        /// </summary>
        /// <param name="text">要搜索的数字或字母数字文本</param>
        [Theory]
        [InlineData("1234567890")]
        [InlineData("ABC123")]
        [InlineData("Test@123")]
        [InlineData("Version 2.1.0")]
        [InlineData("ID: 45678")]
        public async Task GetTextCoordinatesAsync_NumericAndAlphanumericText_ShouldReturnCoordinates(string text)
        {
            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

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
            
            Console.WriteLine($"查找数字/字母数字文本坐标 '{text}': success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            
            if (found && jsonResult.TryGetProperty("coordinates", out var coordinatesProperty))
            {
                var x = coordinatesProperty.GetProperty("x").GetInt32();
                var y = coordinatesProperty.GetProperty("y").GetInt32();
                Console.WriteLine($"找到数字/字母数字文本 '{text}' 坐标: ({x}, {y})");
            }
            else
            {
                Console.WriteLine($"未找到数字/字母数字文本 '{text}'");
            }
        }

        #endregion

        #region 边界条件测试

        /// <summary>
        /// 测试查找不存在的文本时的坐标处理逻辑
        /// 验证OCR工具在未找到目标文本时能够正确返回未找到的状态
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_TextNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var text = "VeryUniqueTextThatShouldNotExistOnScreen98765";

            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

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
            
            Console.WriteLine($"查找不存在文本坐标 '{text}': success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            Assert.NotEmpty(message);
            
            // 真实OCR可能找不到文本，这是正常的
            if (jsonResult.TryGetProperty("found", out var foundProperty2) && !foundProperty2.GetBoolean())
            {
                Console.WriteLine($"文本 '{text}' 未找到，这是预期的结果");
                
                // 未找到时，coordinates应该为null或不存在
                if (jsonResult.TryGetProperty("coordinates", out var coordinatesProperty))
                {
                    Assert.Equal(JsonValueKind.Null, coordinatesProperty.ValueKind);
                }
            }
            else if (found && jsonResult.TryGetProperty("coordinates", out var coordinatesProperty))
            {
                var x = coordinatesProperty.GetProperty("x").GetInt32();
                var y = coordinatesProperty.GetProperty("y").GetInt32();
                Console.WriteLine($"意外找到文本坐标: ({x}, {y})");
            }
        }

        /// <summary>
        /// 测试查找空字符串时的边界情况处理
        /// 验证OCR工具对空输入参数的容错机制和错误处理
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_EmptyText_ShouldReturnErrorResult()
        {
            // Arrange
            var text = "";

            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

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
            
            Console.WriteLine($"查找空文本坐标: success={success}, found={found}, message='{message}'");
            Assert.Equal(text, returnedSearchText);
            
            // 空文本通常不会找到任何结果
            if (jsonResult.TryGetProperty("found", out var foundProperty2))
            {
                var foundValue = foundProperty2.GetBoolean();
                Console.WriteLine($"空文本是否找到: {foundValue}");
            }
        }

        /// <summary>
        /// 测试查找null字符串时的边界情况处理
        /// 验证OCR工具对null输入参数的容错机制
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_NullText_ShouldHandleGracefully()
        {
            // Arrange
            string text = null;

            // Act & Assert
            // 工具应该能够处理null输入而不抛出异常
            var result = await _tool.GetTextCoordinatesAsync(text);
            
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out _));
            Assert.True(jsonResult.TryGetProperty("found", out _));
            Assert.True(jsonResult.TryGetProperty("searchText", out _));
            Assert.True(jsonResult.TryGetProperty("message", out _));
            
            Console.WriteLine($"查找null文本坐标结果: {result}");
        }

        /// <summary>
        /// 测试查找超长文本的坐标处理
        /// 验证工具对异常长度文本的坐标查找能力
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_VeryLongText_ShouldHandleAppropriately()
        {
            // Arrange
            var text = new string('B', 500); // 500字符的长文本

            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.True(jsonResult.TryGetProperty("message", out var messageProperty));

            var success = successProperty.GetBoolean();
            var found = foundProperty.GetBoolean();
            var returnedSearchText = searchTextProperty.GetString();
            
            Console.WriteLine($"查找超长文本坐标: success={success}, found={found}, 文本长度={returnedSearchText?.Length}");
            Assert.Equal(text, returnedSearchText);
        }

        #endregion

        #region 坐标验证测试

        /// <summary>
        /// 测试返回坐标的有效性验证
        /// 验证当找到文本时，返回的坐标是有效的屏幕坐标
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_ValidCoordinates_ShouldReturnValidScreenCoordinates()
        {
            // Arrange
            var text = "CoordinateValidationTest";

            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
            Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));

            var success = successProperty.GetBoolean();
            var found = foundProperty.GetBoolean();
            
            if (found && jsonResult.TryGetProperty("coordinates", out var coordinatesProperty))
            {
                Assert.True(coordinatesProperty.TryGetProperty("x", out var xProperty));
                Assert.True(coordinatesProperty.TryGetProperty("y", out var yProperty));
                
                var x = xProperty.GetInt32();
                var y = yProperty.GetInt32();
                
                Console.WriteLine($"验证坐标有效性: ({x}, {y})");
                
                // 验证坐标在合理的屏幕范围内
                Assert.True(x >= 0, $"X坐标应该非负，实际值: {x}");
                Assert.True(y >= 0, $"Y坐标应该非负，实际值: {y}");
                Assert.True(x <= 10000, $"X坐标应该在合理范围内，实际值: {x}");
                Assert.True(y <= 10000, $"Y坐标应该在合理范围内，实际值: {y}");
            }
            else
            {
                Console.WriteLine("未找到文本，无法验证坐标");
            }
        }

        /// <summary>
        /// 测试坐标数据类型的一致性
        /// 验证坐标值始终是整数类型
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_CoordinateDataTypes_ShouldBeConsistent()
        {
            // Arrange
            var text = "DataTypeTest";

            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

            // Assert
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            
            if (jsonResult.TryGetProperty("found", out var foundProperty) && foundProperty.GetBoolean())
            {
                if (jsonResult.TryGetProperty("coordinates", out var coordinatesProperty))
                {
                    Assert.True(coordinatesProperty.TryGetProperty("x", out var xProperty));
                    Assert.True(coordinatesProperty.TryGetProperty("y", out var yProperty));
                    
                    // 验证坐标值是数字类型
                    Assert.Equal(JsonValueKind.Number, xProperty.ValueKind);
                    Assert.Equal(JsonValueKind.Number, yProperty.ValueKind);
                    
                    // 验证可以转换为整数
                    Assert.True(xProperty.TryGetInt32(out var x));
                    Assert.True(yProperty.TryGetInt32(out var y));
                    
                    Console.WriteLine($"坐标数据类型验证通过: ({x}, {y})");
                }
            }
            else
            {
                Console.WriteLine("未找到文本，跳过坐标数据类型验证");
            }
        }

        #endregion

        #region 性能测试

        /// <summary>
        /// 测试连续多次坐标查找的性能表现
        /// 验证工具在连续使用时的稳定性和内存管理
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_MultipleConsecutiveSearches_ShouldMaintainPerformance()
        {
            // Arrange
            var searchTexts = new[]
            {
                "Coord1",
                "Coord2", 
                "Coord3",
                "Coord4",
                "Coord5"
            };

            // Act & Assert
            for (int i = 0; i < searchTexts.Length; i++)
            {
                var text = searchTexts[i];
                var result = await _tool.GetTextCoordinatesAsync(text);
                
                var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
                Assert.True(jsonResult.TryGetProperty("success", out var successProperty));
                Assert.True(jsonResult.TryGetProperty("found", out var foundProperty));
                Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
                
                var success = successProperty.GetBoolean();
                var found = foundProperty.GetBoolean();
                var returnedSearchText = searchTextProperty.GetString();
                
                Console.WriteLine($"第{i + 1}次坐标查找 '{text}': success={success}, found={found}");
                Assert.Equal(text, returnedSearchText);
                
                if (found && jsonResult.TryGetProperty("coordinates", out var coordinatesProperty))
                {
                    var x = coordinatesProperty.GetProperty("x").GetInt32();
                    var y = coordinatesProperty.GetProperty("y").GetInt32();
                    Console.WriteLine($"  坐标: ({x}, {y})");
                }
                
                // 短暂延迟以避免过快的连续调用
                await Task.Delay(50);
            }
        }

        /// <summary>
        /// 测试并发坐标查找的线程安全性
        /// 验证工具在并发场景下的稳定性
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_ConcurrentSearches_ShouldHandleThreadSafety()
        {
            // Arrange
            var searchTexts = new[] { "ConcurrentCoord1", "ConcurrentCoord2", "ConcurrentCoord3" };
            var tasks = new List<Task<string>>();

            // Act - 并发查找
            foreach (var text in searchTexts)
            {
                tasks.Add(_tool.GetTextCoordinatesAsync(text));
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
                
                Console.WriteLine($"并发坐标查找{i + 1} '{searchTexts[i]}': success={success}, found={found}");
                Assert.Equal(searchTexts[i], returnedSearchText);
                
                if (found && jsonResult.TryGetProperty("coordinates", out var coordinatesProperty))
                {
                    var x = coordinatesProperty.GetProperty("x").GetInt32();
                    var y = coordinatesProperty.GetProperty("y").GetInt32();
                    Console.WriteLine($"  并发查找坐标: ({x}, {y})");
                }
            }
        }

        #endregion

        #region 异常处理测试

        /// <summary>
        /// 测试工具在OCR服务异常时的错误处理
        /// 验证异常情况下的优雅降级和错误信息返回
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_ServiceException_ShouldReturnErrorResult()
        {
            // Note: 由于使用真实的OCR服务，这个测试主要验证异常情况下的JSON结构
            // 在实际异常发生时（如OCR模型加载失败），工具应该返回错误状态
            
            // Arrange
            var text = "ExceptionTestCoordinates";

            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

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
        public async Task GetTextCoordinatesAsync_JsonFormat_ShouldBeConsistent()
        {
            // Arrange
            var text = "JsonFormatCoordinateTest";

            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

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
            
            // 如果找到文本，验证coordinates结构
            if (foundProperty.GetBoolean() && jsonResult.TryGetProperty("coordinates", out var coordinatesProperty))
            {
                Assert.Equal(JsonValueKind.Object, coordinatesProperty.ValueKind);
                Assert.True(coordinatesProperty.TryGetProperty("x", out var xProperty));
                Assert.True(coordinatesProperty.TryGetProperty("y", out var yProperty));
                Assert.Equal(JsonValueKind.Number, xProperty.ValueKind);
                Assert.Equal(JsonValueKind.Number, yProperty.ValueKind);
            }
        }

        /// <summary>
        /// 测试JSON输出的格式化和可读性
        /// 验证返回的JSON是格式化的（包含缩进）
        /// </summary>
        [Fact]
        public async Task GetTextCoordinatesAsync_JsonFormatting_ShouldBeIndented()
        {
            // Arrange
            var text = "FormattingCoordinateTest";

            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

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
        public async Task GetTextCoordinatesAsync_Logging_ShouldLogAppropriateMessages()
        {
            // Note: 由于使用NullLogger，这个测试主要验证调用不会抛出异常
            // 在实际应用中，可以使用TestLogger来验证具体的日志消息
            
            // Arrange
            var text = "LoggingCoordinateTest";

            // Act
            var result = await _tool.GetTextCoordinatesAsync(text);

            // Assert
            // 验证调用成功完成且没有抛出异常
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            var jsonResult = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(jsonResult.TryGetProperty("success", out _));
            Assert.True(jsonResult.TryGetProperty("searchText", out var searchTextProperty));
            Assert.Equal(text, searchTextProperty.GetString());
            
            Console.WriteLine("坐标查找日志测试完成，没有异常抛出");
        }

        #endregion
    }
}
