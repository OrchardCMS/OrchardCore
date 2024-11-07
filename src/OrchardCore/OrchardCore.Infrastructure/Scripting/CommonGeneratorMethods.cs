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
            var neededLength = GetByteArrayLengthFromBase64(encoded);
            Span<byte> bytes = new byte[neededLength];

            if (!Convert.TryFromBase64String(encoded, bytes, out var bytesWritten))
            {
                throw new FormatException("Invalid Base64 string.");
            }

            return Encoding.UTF8.GetString(bytes);
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
            var neededLength = GetByteArrayLengthFromBase64(encoded);

            Span<byte> bytes = new byte[neededLength];

            if (!Convert.TryFromBase64String(encoded, bytes, out var _))
            {
                throw new FormatException("Invalid Base64 string.");
            }

            using var stream = MemoryStreamFactory.GetStream();
            stream.Write(bytes);
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

    // Base64 string length gives us the original byte length. It encodes 3 bytes into 4 characters.
    // The formula to calculate the number of bytes from the base64 string length is:
    private static int GetByteArrayLengthFromBase64(string base64String)
    {
        return (base64String.Length * 3) / 4;
    }
}
