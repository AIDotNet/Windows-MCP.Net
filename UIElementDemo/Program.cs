using Microsoft.Extensions.Logging;
using WindowsMCP.Net.Services;

namespace UIElementDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Windows MCP.Net UI元素识别功能演示");
            Console.WriteLine("使用Windows API实现，替代System.Windows.Automation");
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine();

            // 创建日志记录器
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
            var logger = loggerFactory.CreateLogger<DesktopService>();
            var desktopService = new DesktopService(logger);

            try
            {
                // 演示1: 通过文本查找UI元素
                Console.WriteLine("=== 演示1: 通过文本查找UI元素 ===");
                var result1 = await desktopService.FindElementByTextAsync("记事本");
                Console.WriteLine($"查找包含'记事本'文本的窗口: {result1}");
                Console.WriteLine();

                // 演示2: 通过类名查找UI元素
                Console.WriteLine("=== 演示2: 通过类名查找UI元素 ===");
                var result2 = await desktopService.FindElementByClassNameAsync("Shell_TrayWnd");
                Console.WriteLine($"查找任务栏窗口: {result2}");
                Console.WriteLine();

                // 演示3: 获取指定坐标的元素属性
                Console.WriteLine("=== 演示3: 获取指定坐标的元素属性 ===");
                var result3 = await desktopService.GetElementPropertiesAsync(100, 100);
                Console.WriteLine($"坐标(100,100)处的元素属性: {result3}");
                Console.WriteLine();

                // 演示4: 等待元素出现
                Console.WriteLine("=== 演示4: 等待元素出现 ===");
                var result4 = await desktopService.WaitForElementAsync("Shell_TrayWnd", "className", 2000);
                Console.WriteLine($"等待任务栏窗口出现: {result4}");
                Console.WriteLine();

                Console.WriteLine("演示完成！所有UI元素识别方法已成功使用Windows API实现。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"演示过程中发生错误: {ex.Message}");
            }

            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
    }
}
