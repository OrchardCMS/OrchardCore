using System.IO.Compression;
using System.Net;

namespace OrchardCore.Scripting;

public class CommonGeneratorMethods : IGlobalMethodProvider
{
    private static readonly GlobalMethod[] _allMethods;

    public IEnumerable<GlobalMethod> GetMethods() => _allMethods;

    static CommonGeneratorMethods()
    {
        _allMethods = [_base64, _html, _gZip];
    }

    internal static readonly GlobalMethod _base64 = new()
    {
        Name = "base64",
        Method = serviceProvider => (Func<string, string>)(encoded =>
        {
            return Base64.FromUTF8Base64String(encoded);
        }),
    };

    internal static readonly GlobalMethod _html = new()
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
    internal static readonly GlobalMethod _gZip = new()
    {
        Name = "gzip",
        Method = serviceProvider => (Func<string, string>)(encoded =>
        {
            var compressedStream = Base64.DecodedToStream(encoded);
            compressedStream.Seek(0, SeekOrigin.Begin);

            using var gZip = new GZipStream(compressedStream, CompressionMode.Decompress, leaveOpen: true);

            // The decompressed stream will be bigger that the source.
            using var uncompressedStream = MemoryStreamFactory.GetStream((int)compressedStream.Length);
            gZip.CopyTo(uncompressedStream);

            return Convert.ToBase64String(uncompressedStream.GetBuffer(), 0, (int)uncompressedStream.Length);
        }),
    };
}
