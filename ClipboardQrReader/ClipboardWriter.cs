namespace ClipboardQrReader;

/// <summary>
/// Writes text content to the system clipboard on an STA thread.
/// </summary>
public class ClipboardWriter
{
    /// <summary>
    /// Writes the given text to the system clipboard as plain text.
    /// Executes on an STA thread as required by WinForms Clipboard API.
    /// </summary>
    /// <param name="text">The text to write to the clipboard.</param>
    /// <returns>True if the write succeeded; false if an error occurred.</returns>
    public bool Write(string text)
    {
        bool success = false;
        var thread = new Thread(() =>
        {
            try
            {
                System.Windows.Forms.Clipboard.SetText(text);
                success = true;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log($"ClipboardWriter error: {ex.Message}");
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        return success;
    }
}
