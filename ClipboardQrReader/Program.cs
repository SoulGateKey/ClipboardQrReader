using ClipboardQrReader;

// Global exception handler to prevent crashes
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
    ConsoleLogger.Log($"未处理异常：{e.ExceptionObject}");

// Initialize all modules
var qrDecoder = new QrDecoder();
var clipboardWriter = new ClipboardWriter();
var notificationService = new NotificationService();
var monitor = new ClipboardMonitor(qrDecoder, clipboardWriter, notificationService);

// Handle Ctrl+C gracefully (Requirement 7.2)
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    monitor.Stop();
    cts.Cancel();
};

// Requirement 7.1: Log startup message
ConsoleLogger.Log("剪贴板 QR Code 监听程序已启动，按 Ctrl+C 退出。");
monitor.Start();

// Block main thread until Ctrl+C
try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException)
{
    // Expected on Ctrl+C
}

// Requirement 7.3: Log shutdown message
ConsoleLogger.Log("程序已退出。");
