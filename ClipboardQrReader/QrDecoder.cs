using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ClipboardQrReader.Models;
using ZXing;
using ZXing.Common;

namespace ClipboardQrReader;

/// <summary>
/// Decodes QR Codes from bitmap images using ZXing.NET.
/// </summary>
public class QrDecoder
{
    /// <summary>
    /// Attempts to decode a QR Code from the given bitmap image.
    /// </summary>
    /// <param name="image">The bitmap image to scan.</param>
    /// <returns>
    /// QrDecodeResult with Success=true and the decoded text if a QR Code is found;
    /// QrDecodeResult with Success=false if no QR Code is found or an error occurs.
    /// </returns>
    public QrDecodeResult Decode(Bitmap image)
    {
        try
        {
            // Convert bitmap to RGB byte array for ZXing
            var width = image.Width;
            var height = image.Height;

            // Lock bits and extract RGB data
            var bitmapData = image.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            byte[] rgbBytes;
            try
            {
                int stride = bitmapData.Stride;
                int byteCount = Math.Abs(stride) * height;
                byte[] rawBytes = new byte[byteCount];
                Marshal.Copy(bitmapData.Scan0, rawBytes, 0, byteCount);

                // Convert BGRA (Format32bppArgb on Windows) to RGB
                rgbBytes = new byte[width * height * 3];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcIdx = y * stride + x * 4;
                        int dstIdx = (y * width + x) * 3;
                        rgbBytes[dstIdx]     = rawBytes[srcIdx + 2]; // R
                        rgbBytes[dstIdx + 1] = rawBytes[srcIdx + 1]; // G
                        rgbBytes[dstIdx + 2] = rawBytes[srcIdx];     // B
                    }
                }
            }
            finally
            {
                image.UnlockBits(bitmapData);
            }

            var luminanceSource = new RGBLuminanceSource(rgbBytes, width, height);
            var binarizer = new HybridBinarizer(luminanceSource);
            var binaryBitmap = new BinaryBitmap(binarizer);

            var reader = new ZXing.QrCode.QRCodeReader();
            var hints = new Dictionary<DecodeHintType, object>
            {
                { DecodeHintType.TRY_HARDER, true }
            };
            var result = reader.decode(binaryBitmap, hints);

            if (result != null)
            {
                return new QrDecodeResult(true, result.Text, null);
            }

            return new QrDecodeResult(false, null, null);
        }
        catch (ReaderException)
        {
            // ZXing throws ReaderException (including NotFoundException) when no barcode found
            return new QrDecodeResult(false, null, null);
        }
        catch (Exception ex)
        {
            return new QrDecodeResult(false, null, ex.Message);
        }
    }
}
