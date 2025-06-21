using System.IO.Compression;
using System.Text.Json;
using Microsoft.IO;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents;

/// <summary>
/// Serializes and deserializes an <see cref="IDocument"/> into and from a sequence of bytes.
/// </summary>
public sealed class DefaultDocumentSerializer : IDocumentSerializer
{
    private const string StreamTag = nameof(DefaultDocumentSerializer);

    private static readonly byte[] _gZipHeaderBytes = [0x1f, 0x8b];

    private readonly JsonSerializerOptions _serializerOptions;

    public DefaultDocumentSerializer(JsonSerializerOptions serializerOptions)
    {
        _serializerOptions = serializerOptions;
    }

    public async Task<byte[]> SerializeAsync<TDocument>(TDocument document, int compressThreshold = int.MaxValue)
        where TDocument : class, IDocument, new()
    {
        using var utf8Stream = MemoryStreamFactory.GetStream(StreamTag);
        byte[] result;

        await JsonSerializer.SerializeAsync(utf8Stream, document, _serializerOptions).ConfigureAwait(false);
        utf8Stream.Seek(0, SeekOrigin.Begin);

        if (utf8Stream.Length >= compressThreshold)
        {
            using var stream = MemoryStreamFactory.GetStream(StreamTag);
            await CompressAsync(utf8Stream, stream).ConfigureAwait(false);

            result = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            await stream.CopyToAsync(new MemoryStream(result)).ConfigureAwait(false);
        }
        else
        {
            result = new byte[utf8Stream.Length];
            await utf8Stream.CopyToAsync(new MemoryStream(result)).ConfigureAwait(false);
        }

        return result;
    }

    public async Task<TDocument> DeserializeAsync<TDocument>(byte[] data)
        where TDocument : class, IDocument, new()
    {
        TDocument document;

        if (IsCompressed(data))
        {
            // Assume the decompressed data could fill a twice as big buffer.
            var stream = MemoryStreamFactory.GetStream(data.Length * 2, StreamTag);

            await DecompressAsync(data, stream).ConfigureAwait(false);
            stream.Seek(0, SeekOrigin.Begin);

            document = await JsonSerializer.DeserializeAsync<TDocument>(stream, _serializerOptions).ConfigureAwait(false);
        }
        else
        {
            document = JsonSerializer.Deserialize<TDocument>(data, _serializerOptions);
        }

        return document;
    }

    internal static bool IsCompressed(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return data.AsSpan().StartsWith(_gZipHeaderBytes);
    }

    internal static async Task CompressAsync(Stream source, RecyclableMemoryStream output)
    {
        using var gZip = new GZipStream(output, CompressionMode.Compress, leaveOpen: true);
        await source.CopyToAsync(gZip).ConfigureAwait(false);
    }

    internal static async Task DecompressAsync(byte[] data, RecyclableMemoryStream output)
    {
        using var input = new MemoryStream(data);
        using var gZip = new GZipStream(input, CompressionMode.Decompress);
        await gZip.CopyToAsync(output).ConfigureAwait(false);
    }
}
