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
            return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
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

            using var memoryStream = MemoryStreamFactory.GetStream(neededLength);
            var bytes = memoryStream.GetSpan(neededLength);

            if (!Convert.TryFromBase64String(encoded, bytes, out var _))
            {
                throw new FormatException("Invalid Base64 string.");
            }

            using var stream = MemoryStreamFactory.GetStream();
            stream.Write(bytes);
            stream.Seek(0, SeekOrigin.Begin);

            using var gZip = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);

            using var decompressed = stream;
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

    // The length of a Base64-encoded string corresponds to the original byte length. 
    // Base64 encoding converts every 3 bytes of data into 4 characters. 
    // To calculate the original byte length from the Base64 string, use the formula: 
    // (length * 3) / 4. This ensures the correct byte count by multiplying the length by 3 
    // before dividing by 4.
    private static int GetByteArrayLengthFromBase64(string base64String)
    {
        return base64String.Length * 3 / 4;
    }
}
