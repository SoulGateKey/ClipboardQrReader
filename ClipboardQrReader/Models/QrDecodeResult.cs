namespace ClipboardQrReader.Models;

/// <summary>
/// Represents the result of a QR Code decode operation.
/// </summary>
/// <param name="Success">True if a QR Code was successfully decoded.</param>
/// <param name="Text">The decoded text content. Non-null when Success is true.</param>
/// <param name="ErrorMessage">Optional error description when Success is false.</param>
public record QrDecodeResult(
    bool Success,
    string? Text,
    string? ErrorMessage
);
