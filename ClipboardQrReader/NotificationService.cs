using Microsoft.Toolkit.Uwp.Notifications;

namespace ClipboardQrReader;

/// <summary>
/// Sends Windows Toast notifications.
/// </summary>
public class NotificationService
{
    private const int MaxBodyLength = 200;

    /// <summary>
    /// Shows a Windows Toast notification with the given title and body.
    /// The body will be truncated if it exceeds the maximum display length.
    /// </summary>
    public void Show(string title, string body)
    {
        var truncatedBody = Truncate(body, MaxBodyLength);
        new ToastContentBuilder()
            .AddText(title)
            .AddText(truncatedBody)
            .Show();
    }

    /// <summary>
    /// Truncates text to the specified maximum length, appending '\u2026' if truncated.
    /// </summary>
    internal string Truncate(string text, int maxLength)
    {
        if (text.Length > maxLength)
            return text[..(maxLength - 1)] + "\u2026";
        return text;
    }
}
