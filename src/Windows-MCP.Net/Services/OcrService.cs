using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using Sdcb.OpenVINO.PaddleOCR;
using Sdcb.OpenVINO.PaddleOCR.Models.Online;
using Sdcb.OpenVINO.PaddleOCR.Models;
using System.Runtime.InteropServices;
using Interface;


namespace WindowsMCP.Net.Services;

/// <summary>
/// Implementation of OCR services using PaddleOCR.
/// </summary>
public class OcrService : IOcrService, IDisposable
{
    private readonly ILogger<OcrService> _logger;
    private static readonly Lazy<OcrService> _instance = new Lazy<OcrService>(() => new OcrService());
    private static readonly object _lock = new object();
    private static FullOcrModel? _model = null;
    private bool _disposed = false;

    // Windows API imports for screen capture
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hGDIObj);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hDC);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    // System metrics constants
    private const int SM_CXSCREEN = 0; // Width of the screen
    private const int SM_CYSCREEN = 1; // Height of the screen

    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static OcrService Instance => _instance.Value;

    /// <summary>
    /// 初始化OCR服务
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public OcrService(ILogger<OcrService> logger = null)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<OcrService>.Instance;
    }

    /// <summary>
    /// 私有构造函数，用于单例模式
    /// </summary>
    private OcrService()
    {
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<OcrService>.Instance;
    }

    /// <summary>
    /// Initialize the OCR model asynchronously.
    /// </summary>
    private async Task InitializeModelAsync()
        {
            if (_model == null)
            {
                lock (_lock)
                {
                    if (_model == null)
                    {
                        try
                        {
                            _logger.LogInformation("Initializing OCR model...");
                            
                            // 诊断信息：检查OpenCV是否可用
                            try
                            {
                                var version = Cv2.GetVersionString();
                                _logger.LogInformation("OpenCV version: {Version}", version);
                            }
                            catch (Exception cvEx)
                            {
                                _logger.LogError(cvEx, "Failed to get OpenCV version - native libraries may not be loaded correctly");
                                throw new InvalidOperationException("OpenCV native libraries are not available. This may be due to missing runtime dependencies in the dnx environment.", cvEx);
                            }
                            
                            // 诊断信息：检查运行时环境
                            var runtimeInfo = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
                            var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
                            _logger.LogInformation("Runtime: {Runtime}, Architecture: {Architecture}", runtimeInfo, architecture);
                            
                            _logger.LogInformation("Downloading OCR model...");
                            _model = OnlineFullModels.ChineseV4.DownloadAsync().Result;
                            _logger.LogInformation("OCR model downloaded and initialized successfully");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to initialize OCR model. Error: {Message}", ex.Message);
                            throw;
                        }
                    }
                }
            }
        }

    /// <summary>
    /// Extract text from a specific region of the screen.
    /// </summary>
    public async Task<(string Text, int Status)> ExtractTextFromRegionAsync(int x, int y, int width, int height, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Extracting text from screen region: ({X}, {Y}, {Width}, {Height})", x, y, width, height);
            
            await InitializeModelAsync();
            
            using var bitmap = CaptureScreenRegion(x, y, width, height);
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            
            var (text, status) = await ExtractTextFromImageAsync(stream, cancellationToken);
            return (text, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from screen region");
            return (string.Empty, 1);
        }
    }

    /// <summary>
    /// Extract text from the entire screen.
    /// </summary>
    public async Task<(string Text, int Status)> ExtractTextFromScreenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Extracting text from entire screen");
            
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);
            return await ExtractTextFromRegionAsync(0, 0, screenWidth, screenHeight, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from screen");
            return (string.Empty, 1);
        }
    }

    /// <summary>
    /// Find specific text on the screen.
    /// </summary>
    public async Task<(bool Found, int Status)> FindTextOnScreenAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching for text on screen: {Text}", text);
            
            var (extractedText, status) = await ExtractTextFromScreenAsync(cancellationToken);
            if (status != 0)
            {
                return (false, status);
            }
            
            bool found = extractedText.Contains(text, StringComparison.OrdinalIgnoreCase);
            return (found, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for text on screen");
            return (false, 1);
        }
    }

    /// <summary>
    /// Get coordinates of specific text on the screen.
    /// </summary>
    public async Task<(System.Drawing.Point? Coordinates, int Status)> GetTextCoordinatesAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting coordinates for text: {Text}", text);
            
            await InitializeModelAsync();
            
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);
            using var bitmap = CaptureScreenRegion(0, 0, screenWidth, screenHeight);
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            
            using (PaddleOcrAll all = new(_model)
            {
                AllowRotateDetection = true,
                Enable180Classification = true,
            })
            {
                Mat src = Cv2.ImDecode(StreamToByte(stream), ImreadModes.Color);
                PaddleOcrResult result = all.Run(src);
                
                // 查找包含指定文本的区域
                foreach (var region in result.Regions)
                {
                    if (region.Text.Contains(text, StringComparison.OrdinalIgnoreCase))
                    {
                        // 返回文本区域的中心点坐标
                        var centerX = (int)region.Rect.Center.X;
                        var centerY = (int)region.Rect.Center.Y;
                        return (new System.Drawing.Point(centerX, centerY), 0);
                    }
                }
            }
            
            return (null, 0); // 未找到文本
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting text coordinates");
            return (null, 1);
        }
    }

    /// <summary>
    /// Extract text from an image stream.
    /// </summary>
    public async Task<(string Text, int Status)> ExtractTextFromImageAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        try
        {
            await InitializeModelAsync();

            using (PaddleOcrAll all = new(_model)
            {
                AllowRotateDetection = true,
                Enable180Classification = true,
            })
            {
                Mat src = Cv2.ImDecode(StreamToByte(imageStream), ImreadModes.Color);
                PaddleOcrResult result = all.Run(src);
                return (result.Text, 0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from image: {Message}", ex.Message);
            return (string.Empty, 1);
        }
    }

    /// <summary>
    /// 截取屏幕指定区域
    /// </summary>
    private Bitmap CaptureScreenRegion(int x, int y, int width, int height)
    {
        IntPtr desktopWindow = GetDesktopWindow();
        IntPtr desktopDC = GetWindowDC(desktopWindow);
        IntPtr memoryDC = CreateCompatibleDC(desktopDC);
        IntPtr bitmap = CreateCompatibleBitmap(desktopDC, width, height);
        IntPtr oldBitmap = SelectObject(memoryDC, bitmap);

        BitBlt(memoryDC, 0, 0, width, height, desktopDC, x, y, 0x00CC0020); // SRCCOPY

        SelectObject(memoryDC, oldBitmap);
        DeleteDC(memoryDC);
        ReleaseDC(desktopWindow, desktopDC);

        Bitmap result = Image.FromHbitmap(bitmap);
        DeleteObject(bitmap);

        return result;
    }

    /// <summary>
    /// 将Stream转换为byte数组
    /// </summary>
    private byte[] StreamToByte(Stream stream)
    {
        using (var memoryStream = new MemoryStream())
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                memoryStream.Write(buffer, 0, bytesRead);
            }
            return memoryStream.ToArray();
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源的具体实现
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    _model = null;
                }
            }
            _disposed = true;
        }
    }
}