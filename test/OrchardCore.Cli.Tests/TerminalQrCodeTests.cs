using ZXing;

namespace OrchardCore.Cli.Tests;

public class TerminalQrCodeTests
{
    [Theory]
    [InlineData("https://cms.example.com/connect/verify")]
    [InlineData("https://cms.example.com/team/connect/verify?user_code=6738-0585-5256")]
    [InlineData("https://cms.example.com/tenant%20one/connect/verify?user_code=A%2BB%26C")]
    [InlineData("http://127.0.0.1:5000/connect/verify?user_code=1234-5678")]
    public void Render_TerminalCellsDecodeToExactUrl(string url)
    {
        var graphic = TerminalQrCode.Render(url, 79);
        Assert.NotNull(graphic);
        Assert.Equal(url, Decode(graphic));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    [InlineData(29)]
    public void Render_InsufficientWidth_OmitsQrCode(int width)
    {
        Assert.Null(TerminalQrCode.Render("https://cms.example.com/team/connect/verify?user_code=6738-0585-5256", width));
    }

    [Fact]
    public void Render_OversizedUrl_OmitsQrCode()
    {
        Assert.Null(TerminalQrCode.Render("https://cms.example.com/" + new string('a', 3000), 120));
    }

    internal static string Decode(string graphic)
    {
        var lines = graphic.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.TrimEnd('\r')).ToArray();
        foreach (var line in lines)
        {
            Assert.StartsWith("\u001b[30;107m", line);
            Assert.EndsWith("\u001b[0m", line);
        }
        var cells = lines.Select(line => line[9..^4]).ToArray();
        var columns = cells[0].Length;
        Assert.True(columns <= 79);
        Assert.All(cells, line => Assert.Equal(columns, line.Length));
        Assert.All(cells[0], cell => Assert.Equal(' ', cell));

        const int scale = 8;
        var width = columns * scale;
        var height = cells.Length * 2 * scale;
        var pixels = new byte[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var cell = cells[y / (2 * scale)][x / scale];
                var top = y / scale % 2 == 0;
                var dark = cell == '█' || (top ? cell == '▀' : cell == '▄');
                pixels[y * width + x] = dark ? (byte)0 : (byte)255;
            }
        }

        // Decode the rendered terminal cells with an independent library,
        // including detection and quiet-zone handling, not the encoder matrix.
        var decoded = new BarcodeReaderGeneric().Decode(pixels, width, height, RGBLuminanceSource.BitmapFormat.Gray8);
        Assert.NotNull(decoded);
        Assert.Equal(BarcodeFormat.QR_CODE, decoded.BarcodeFormat);
        return decoded.Text;
    }
}
