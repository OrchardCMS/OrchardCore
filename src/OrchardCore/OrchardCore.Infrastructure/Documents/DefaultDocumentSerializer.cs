using System.IO.Compression;
using System.Text.Json;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents;

/// <summary>
/// Serializes and deserializes an <see cref="IDocument"/> into and from a sequence of bytes.
/// </summary>
public class DefaultDocumentSerializer : IDocumentSerializer
{
    private static readonly byte[] _gZipHeaderBytes = [0x1f, 0x8b];

    private readonly JsonSerializerOptions _serializerOptions;

    public DefaultDocumentSerializer(JsonSerializerOptions serializerOptions)
    {
        _serializerOptions = serializerOptions;
    }

    public Task<byte[]> SerializeAsync<TDocument>(TDocument document, int compressThreshold = int.MaxValue)
        where TDocument : class, IDocument, new()
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(document, _serializerOptions);
        if (data.Length >= compressThreshold)
        {
            data = Compress(data).ToArray();
        }

        return Task.FromResult(data);
    }

    public Task<TDocument> DeserializeAsync<TDocument>(byte[] data)
        where TDocument : class, IDocument, new()
    {
        TDocument document;

        if (IsCompressed(data))
        {
            document = JsonSerializer.Deserialize<TDocument>(Decompress(data), _serializerOptions);
        }
        else
        {
            document = JsonSerializer.Deserialize<TDocument>(data, _serializerOptions);
        }

        return Task.FromResult(document);
    }

    internal static bool IsCompressed(byte[] data)
    {
        // Ensure data is at least as long as the GZip header
        if (data.Length >= _gZipHeaderBytes.Length)
        {
            // Compare the header bytes.
            return data.Take(_gZipHeaderBytes.Length).SequenceEqual(_gZipHeaderBytes);
        }

        return false;
    }

    internal static ReadOnlySpan<byte> Compress(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var output = MemoryStreamFactory.GetStream();
        using var gZip = new GZipStream(output, CompressionMode.Compress);

        input.CopyTo(gZip);

        return output.GetBuffer().AsSpan().Slice(0, (int)gZip.Length);
    }

    internal static ReadOnlySpan<byte> Decompress(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var output = MemoryStreamFactory.GetStream();
        using var gZip = new GZipStream(input, CompressionMode.Decompress);
        gZip.CopyTo(output);

        return output.GetBuffer().AsSpan().Slice(0, (int)gZip.Length);
    }
}
