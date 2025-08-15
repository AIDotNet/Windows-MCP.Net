using System.ComponentModel;
using System.Text.Json;
using Interface;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace WindowsMCP.Net.Tools.OCR;

/// <summary>
/// Tool for extracting text from image files using OCR.
/// </summary>
[McpServerToolType]
public class ExtractTextFromFileTool
{
    private readonly IOcrService _ocrService;
    private readonly ILogger<ExtractTextFromFileTool> _logger;

    /// <summary>
    /// 初始化文件文字识别工具
    /// </summary>
    /// <param name="ocrService">OCR服务</param>
    /// <param name="logger">日志记录器</param>
    public ExtractTextFromFileTool(IOcrService ocrService, ILogger<ExtractTextFromFileTool> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    /// <summary>
    /// 从图像文件中提取文字
    /// </summary>
    /// <param name="path">图像文件路径</param>
    /// <returns>包含识别结果的JSON字符串</returns>
    [McpServerTool, Description("Extract text from image files using OCR")]
    public async Task<string> ExtractTextFromFileAsync(
        [Description("The file path to extract text from")] string path)
    {
        try
        {
            _logger.LogInformation("Extracting text from file: {Path}", path);
            
            // 检查文件是否存在
            if (!File.Exists(path))
            {
                var notFoundResult = new
                {
                    success = false,
                    text = string.Empty,
                    message = $"File not found: {path}"
                };
                return JsonSerializer.Serialize(notFoundResult, new JsonSerializerOptions { WriteIndented = true });
            }

            // 检查文件扩展名是否为支持的图像格式
            var extension = Path.GetExtension(path).ToLowerInvariant();
            var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".webp" };
            
            if (!supportedExtensions.Contains(extension))
            {
                var unsupportedResult = new
                {
                    success = false,
                    text = string.Empty,
                    message = $"Unsupported file format: {extension}. Supported formats: {string.Join(", ", supportedExtensions)}"
                };
                return JsonSerializer.Serialize(unsupportedResult, new JsonSerializerOptions { WriteIndented = true });
            }

            // 读取文件并提取文字
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var (text, status) = await _ocrService.ExtractTextFromImageAsync(fileStream);
            
            var result = new
            {
                success = status == 0,
                text = status == 0 ? text : string.Empty,
                message = status == 0 ? $"Text extracted successfully from file: {Path.GetFileName(path)}" : $"Failed to extract text from file: {Path.GetFileName(path)}",
                filePath = path
            };
            
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when reading file: {Path}", path);
            var accessDeniedResult = new
            {
                success = false,
                text = string.Empty,
                message = $"Access denied: {ex.Message}"
            };
            return JsonSerializer.Serialize(accessDeniedResult, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "IO error when reading file: {Path}", path);
            var ioErrorResult = new
            {
                success = false,
                text = string.Empty,
                message = $"IO error: {ex.Message}"
            };
            return JsonSerializer.Serialize(ioErrorResult, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExtractTextFromFileAsync for file: {Path}", path);
            var errorResult = new
            {
                success = false,
                text = string.Empty,
                message = $"Error extracting text from file: {ex.Message}"
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}