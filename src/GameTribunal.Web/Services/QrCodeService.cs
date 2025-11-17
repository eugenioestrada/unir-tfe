using QRCoder;

namespace GameTribunal.Web.Services;

/// <summary>
/// Service for generating QR codes from URLs.
/// </summary>
public sealed class QrCodeService
{
    /// <summary>
    /// Generates a base64-encoded PNG image of a QR code for the specified data.
    /// </summary>
    /// <param name="data">The data to encode in the QR code.</param>
    /// <param name="pixelsPerModule">The size of each QR code module in pixels. Default is 10.</param>
    /// <returns>A base64-encoded PNG image string.</returns>
    public string GenerateQrCodeBase64(string data, int pixelsPerModule = 10)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(data, nameof(data));

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        var qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);
        return Convert.ToBase64String(qrCodeBytes);
    }
}
