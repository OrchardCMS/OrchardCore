using System.IO.Compression;
using System.Net;
using System.Text;

namespace OrchardCore.Scripting;

public class CommonGeneratorMethods : IGlobalMethodProvider
{
    private static readonly GlobalMethod _base64 = new()
    {
        Name = "base64",
        Method = serviceProvider => (Func<string, string>)(encoded =>
        {
            return Encoding.UTF8.GetString(Str.FromBase64String(encoded));
        }),
    };

    private static readonly GlobalMethod _html = new()
    {
        Name = "html",
        Method = serviceProvider => (Func<string, string>)(encoded =>
        {
            return WebUtility.HtmlDecode(encoded);
        }),
    };

    /// <summary>
    /// Converts a Base64 encoded gzip stream to an uncompressed Base64 string.
    /// See http://www.txtwizard.net/compression.
    /// </summary>
    private readonly GlobalMethod _gZip = new()
    {
        Name = "gzip",
        Method = serviceProvider => (Func<string, string>)(encoded =>
        {
            using var stream = MemoryStreamFactory.GetStream();
            stream.Write(Str.FromBase64String(encoded));
            stream.Seek(0, SeekOrigin.Begin);

            using var gZip = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);

            using var decompressed = MemoryStreamFactory.GetStream();
            var buffer = new byte[1024];
            int nRead;

            while ((nRead = gZip.Read(buffer, 0, buffer.Length)) > 0)
            {
                decompressed.Write(buffer, 0, nRead);
            }

            return Convert.ToBase64String(decompressed.GetBuffer(), 0, (int)decompressed.Length);
        }),
    };

    public IEnumerable<GlobalMethod> GetMethods()
        => new[] { _base64, _html, _gZip };
}
