using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipboardQrReader;

/// <summary>
/// Monitors the system clipboard for image content changes
/// and coordinates QR Code decoding, notification, and clipboard writing.
/// </summary>
public class ClipboardMonitor : IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private const int WM_CLIPBOARDUPDATE = 0x031D;
    private const uint WM_QUIT = 0x0012;

    private readonly QrDecoder _qrDecoder;
    private readonly ClipboardWriter _clipboardWriter;
    private readonly NotificationService _notificationService;

    private Thread? _messageLoopThread;
    private HiddenWindow? _hiddenWindow;
    private volatile bool _running;
    private bool _disposed;

    private const string NotificationTitle = "已识别 QR Code";
    private const string MsgNoQrCode = "未检测到 QR Code";
    private const string MsgImageReadFailed = "剪贴板图片读取失败";
    private const string MsgWriteFailed = "QR Code 结果写入剪贴板失败";

    public ClipboardMonitor(
        QrDecoder qrDecoder,
        ClipboardWriter clipboardWriter,
        NotificationService notificationService)
    {
        _qrDecoder = qrDecoder;
        _clipboardWriter = clipboardWriter;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Starts clipboard monitoring by registering a Win32 clipboard listener
    /// and running the message loop on an STA thread.
    /// </summary>
    public void Start()
    {
        if (_running)
            return;

        _running = true;
        _messageLoopThread = new Thread(MessageLoopProc);
        _messageLoopThread.SetApartmentState(ApartmentState.STA);
        _messageLoopThread.IsBackground = true;
        _messageLoopThread.Name = "ClipboardMonitor-STA";
        _messageLoopThread.Start();
    }

    /// <summary>
    /// Stops clipboard monitoring and unregisters the Win32 listener.
    /// </summary>
    public void Stop()
    {
        if (!_running)
            return;

        _running = false;

        // Post WM_QUIT to the hidden window's thread to exit Application.Run()
        var win = _hiddenWindow;
        if (win != null && win.Handle != IntPtr.Zero)
        {
            RemoveClipboardFormatListener(win.Handle);
            Application.ExitThread();
        }

        _messageLoopThread?.Join(TimeSpan.FromSeconds(5));
        _messageLoopThread = null;
    }

    private void MessageLoopProc()
    {
        _hiddenWindow = new HiddenWindow(this);
        _hiddenWindow.CreateHandle(new CreateParams());

        if (!AddClipboardFormatListener(_hiddenWindow.Handle))
        {
            ConsoleLogger.Log($"AddClipboardFormatListener 失败，错误码：{Marshal.GetLastWin32Error()}");
        }

        Application.Run();

        // Clean up window after message loop ends
        if (_hiddenWindow.Handle != IntPtr.Zero)
        {
            RemoveClipboardFormatListener(_hiddenWindow.Handle);
            _hiddenWindow.DestroyHandle();
        }
        _hiddenWindow = null;
    }

    /// <summary>
    /// Internal callback invoked when the clipboard content changes.
    /// Reads image data, decodes QR Code, writes result, and shows notification.
    /// </summary>
    private void OnClipboardChanged()
    {
        // Requirement 1.2, 2.1: Check if clipboard contains an image
        if (!Clipboard.ContainsImage())
        {
            // Requirement 1.4: Ignore non-image content and continue listening
            return;
        }

        Bitmap? image = null;
        try
        {
            // Requirement 2.1: Read bitmap from clipboard
            image = Clipboard.GetImage() as Bitmap;
            if (image == null)
            {
                // Requirement 2.3: Log image read failure and continue
                ConsoleLogger.Log(MsgImageReadFailed);
                return;
            }
        }
        catch (Exception ex)
        {
            // Requirement 2.3: Log image read failure and continue
            ConsoleLogger.Log($"{MsgImageReadFailed}: {ex.Message}");
            return;
        }

        try
        {
            // Requirement 2.2: Pass image to QrDecoder
            var result = _qrDecoder.Decode(image);

            if (result.Success && result.Text != null)
            {
                // Requirement 5.2: Write to clipboard before showing notification
                bool writeOk = _clipboardWriter.Write(result.Text);
                if (!writeOk)
                {
                    // Requirement 5.3: Log write failure but still show notification
                    ConsoleLogger.Log(MsgWriteFailed);
                }

                // Requirement 4.1, 4.2, 4.3: Show notification with decoded text
                _notificationService.Show(NotificationTitle, result.Text);
            }
            else if (result.ErrorMessage != null)
            {
                // Requirement 6.2: Log decode error
                ConsoleLogger.Log($"QR Code 识别失败：{result.ErrorMessage}");
            }
            else
            {
                // Requirement 6.1: Log no QR code found
                ConsoleLogger.Log(MsgNoQrCode);
            }
        }
        finally
        {
            image.Dispose();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// A hidden native window used to receive Win32 clipboard change messages.
    /// </summary>
    private sealed class HiddenWindow : NativeWindow
    {
        private readonly ClipboardMonitor _owner;

        public HiddenWindow(ClipboardMonitor owner)
        {
            _owner = owner;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                try
                {
                    _owner.OnClipboardChanged();
                }
                catch (Exception ex)
                {
                    ConsoleLogger.Log($"OnClipboardChanged 异常：{ex.Message}");
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}
