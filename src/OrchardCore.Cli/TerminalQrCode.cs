using System.Text;
using QRCoder;

namespace OrchardCore.Cli;

internal enum QrCodeMode
{
    Auto,
    Always,
    Never,
}

internal static class TerminalQrCode
{
    public static string? RenderPngBase64(string url)
    {
        if (Encoding.UTF8.GetByteCount(url) > 2048)
        {
            return null;
        }

        using var data = QRCodeGenerator.GenerateQrCode(url, QRCodeGenerator.ECCLevel.M);
        using var image = new PngByteQRCode(data);
        return Convert.ToBase64String(image.GetGraphic(4));
    }

    public static int GetMaxWidth(QrCodeMode mode)
    {
        if (mode == QrCodeMode.Never || (mode == QrCodeMode.Auto &&
            (Console.IsErrorRedirected || Console.OutputEncoding.CodePage != Encoding.UTF8.CodePage ||
             string.Equals(System.Environment.GetEnvironmentVariable("TERM"), "dumb", StringComparison.OrdinalIgnoreCase) ||
             !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("NO_COLOR")))))
        {
            return 0;
        }

        try
        {
            var width = Console.WindowWidth;
            return width > 0 ? Math.Min(width - 1, 120) : 79;
        }
        catch (IOException)
        {
            return 79;
        }
    }

    public static string? Render(string url, int maxWidth)
    {
        // QR output is optional. Keep the URL usable when it cannot fit in a
        // terminal or exceeds the encoder's capacity; never wrap a QR code.
        if (maxWidth < 29 || Encoding.UTF8.GetByteCount(url) > 2048)
        {
            return null;
        }

        using var data = QRCodeGenerator.GenerateQrCode(url, QRCodeGenerator.ECCLevel.M);
        var modules = data.ModuleMatrix;
        if (modules.Count > maxWidth)
        {
            return null;
        }

        var result = new StringBuilder();
        // Two module rows per text row keep square proportions in a normal
        // terminal font. Preserve the encoder's four-module quiet zone and
        // force black on white so scanning works with light and dark themes.
        for (var row = 0; row < modules.Count; row += 2)
        {
            result.Append("\u001b[30;107m");
            for (var column = 0; column < modules.Count; column++)
            {
                var top = modules[row][column];
                var bottom = row + 1 < modules.Count && modules[row + 1][column];
                result.Append((top, bottom) switch
                {
                    (true, true) => '█',
                    (true, false) => '▀',
                    (false, true) => '▄',
                    _ => ' ',
                });
            }
            result.Append("\u001b[0m").AppendLine();
        }

        return result.ToString();
    }
}
