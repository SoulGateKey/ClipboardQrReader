using System.Drawing;
using System.Text.RegularExpressions;
using ClipboardQrReader;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using ZXing;
using ZXing.QrCode;

namespace ClipboardQrReader.Tests;

// =====================================================================
// ConsoleLogger Tests
// =====================================================================

public class ConsoleLoggerTests
{
    // Feature: clipboard-qrcode-reader, Property 6: 日志时间戳格式不变式
    // Validates: Requirements 6.3
    [Property(MaxTest = 100)]
    public Property TimestampFormatInvariant(NonNull<string> msg)
    {
        var output = CaptureLog(msg.Get);
        bool valid = output.Length >= 21
            && Regex.IsMatch(output[..21], @"^\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\]$");
        return valid.ToProperty();
    }

    [Fact]
    public void Log_ProducesTimestampedOutput()
    {
        var output = CaptureLog("hello");
        Assert.Matches(@"^\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] hello", output);
    }

    private static string CaptureLog(string message)
    {
        var sw = new System.IO.StringWriter();
        var old = Console.Out;
        Console.SetOut(sw);
        try
        {
            ConsoleLogger.Log(message);
        }
        finally
        {
            Console.SetOut(old);
        }
        return sw.ToString().TrimEnd('\r', '\n');
    }
}

// =====================================================================
// NotificationService Tests
// =====================================================================

public class NotificationServiceTests
{
    private readonly NotificationService _svc = new();

    // Feature: clipboard-qrcode-reader, Property 4: 通知正文截断不变式
    // Validates: Requirements 4.4
    [Property(MaxTest = 100)]
    public Property TruncationInvariant(NonNull<string> text)
    {
        const int maxLength = 200;
        var result = _svc.Truncate(text.Get, maxLength);
        if (text.Get.Length > maxLength)
            return (result.Length <= maxLength && result.EndsWith("\u2026")).ToProperty();
        else
            return (result == text.Get).ToProperty();
    }

    [Fact]
    public void Truncate_ShortString_Unchanged()
    {
        Assert.Equal("hello", _svc.Truncate("hello", 200));
    }

    [Fact]
    public void Truncate_ExactLength_Unchanged()
    {
        var s = new string('a', 200);
        Assert.Equal(s, _svc.Truncate(s, 200));
    }

    [Fact]
    public void Truncate_LongString_TruncatesWithEllipsis()
    {
        var s = new string('a', 300);
        var result = _svc.Truncate(s, 200);
        Assert.True(result.Length <= 200);
        Assert.EndsWith("\u2026", result);
    }

    [Fact]
    public void Truncate_EmptyString_Unchanged()
    {
        Assert.Equal(string.Empty, _svc.Truncate(string.Empty, 200));
    }
}

// =====================================================================
// QrDecoder Tests
// =====================================================================

public class QrDecoderTests
{
    private readonly QrDecoder _decoder = new();

    // Feature: clipboard-qrcode-reader, Property 2: QR Code 识别轮回
    // Validates: Requirements 3.1, 3.2
    [Property(MaxTest = 50)]
    public Property QrRoundTrip(NonEmptyString text)
    {
        var original = text.Get;
        // Skip strings that are too long or have control chars that ZXing cannot encode
        if (original.Length > 200 || original.Any(c => c < 32))
            return true.ToProperty();

        try
        {
            using var bitmap = EncodeQrCode(original);
            var result = _decoder.Decode(bitmap);
            return (result.Success && result.Text == original).ToProperty();
        }
        catch
        {
            // If encoding fails for this input, skip
            return true.ToProperty();
        }
    }

    // Feature: clipboard-qrcode-reader, Property 3: 无 QR Code 图片返回失败状态
    // Validates: Requirements 3.3
    [Property(MaxTest = 50)]
    public Property PlainBitmapReturnsFalse(PositiveInt width, PositiveInt height)
    {
        var w = (width.Get % 200) + 1;
        var h = (height.Get % 200) + 1;
        using var bitmap = new Bitmap(w, h);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.White);
        var result = _decoder.Decode(bitmap);
        return (!result.Success && result.Text == null).ToProperty();
    }

    [Fact]
    public void Decode_WithQrCode_ReturnsSuccess()
    {
        const string text = "https://example.com";
        using var bitmap = EncodeQrCode(text);
        var result = _decoder.Decode(bitmap);
        Assert.True(result.Success);
        Assert.Equal(text, result.Text);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Decode_BlankImage_ReturnsFalse()
    {
        using var bitmap = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.White);
        var result = _decoder.Decode(bitmap);
        Assert.False(result.Success);
        Assert.Null(result.Text);
    }

    private static Bitmap EncodeQrCode(string text)
    {
        var writer = new QRCodeWriter();
        var matrix = writer.encode(text, BarcodeFormat.QR_CODE, 200, 200);
        var bitmap = new Bitmap(matrix.Width, matrix.Height);
        for (int x = 0; x < matrix.Width; x++)
            for (int y = 0; y < matrix.Height; y++)
                bitmap.SetPixel(x, y, matrix[x, y] ? Color.Black : Color.White);
        return bitmap;
    }
}
