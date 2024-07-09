using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// Serializes and deserializes an <see cref="IDocument"/> into and from a sequence of bytes.
    /// </summary>
    public class DefaultDocumentSerializer : IDocumentSerializer
    {
        public static readonly DefaultDocumentSerializer Instance = new();

        public Task<byte[]> SerializeAsync<TDocument>(TDocument document, int compressThreshold = int.MaxValue)
            where TDocument : class, IDocument, new()
        {
            var data = JsonSerializer.SerializeToUtf8Bytes(document, JOptions.Default);
            if (data.Length >= compressThreshold)
            {
                data = Compress(data);
            }

            return Task.FromResult(data);
        }

        public Task<TDocument> DeserializeAsync<TDocument>(byte[] data)
            where TDocument : class, IDocument, new()
        {
            if (IsCompressed(data))
            {
                data = Decompress(data);
            }

            using var ms = new MemoryStream(data);

            var document = JsonSerializer.Deserialize<TDocument>(ms, JOptions.Default);

            return Task.FromResult(document);
        }

        private static readonly byte[] _gZipHeaderBytes = [0x1f, 0x8b];

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

        internal static byte[] Compress(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                input.CopyTo(gzip);
            }

            if (output.TryGetBuffer(out var buffer))
            {
                return buffer.Array;
            }

            return output.ToArray();
        }

        internal static byte[] Decompress(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            {
                gzip.CopyTo(output);
            }

            if (output.TryGetBuffer(out var buffer))
            {
                return buffer.Array;
            }

            return output.ToArray();
        }
    }
}
