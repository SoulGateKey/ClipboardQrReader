namespace ClipboardQrReader;

/// <summary>
/// Provides timestamped console logging.
/// </summary>
public static class ConsoleLogger
{
    /// <summary>
    /// Writes a message to the console with a timestamp prefix.
    /// Format: [YYYY-MM-DD HH:mm:ss] message
    /// </summary>
    public static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {message}");
    }
}
