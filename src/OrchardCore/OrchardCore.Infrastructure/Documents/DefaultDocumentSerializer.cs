using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// Serializes and deserializes an <see cref="IDocument"/> into and from a sequence of bytes.
    /// </summary>
    public class DefaultDocumentSerializer : IDocumentSerialiser
    {
        public static readonly DefaultDocumentSerializer Instance = new();

        private static readonly JsonSerializerSettings _jsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        public DefaultDocumentSerializer()
        {
        }

        public Task<byte[]> SerializeAsync<TDocument>(TDocument document, int compressThreshold = Int32.MaxValue) where TDocument : class, IDocument, new()
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(document, _jsonSettings));

            if (data.Length >= compressThreshold)
            {
                data = Compress(data);
            }

            return Task.FromResult(data);
        }

        public Task<TDocument> DeserializeAsync<TDocument>(byte[] data) where TDocument : class, IDocument, new()
        {
            if (IsCompressed(data))
            {
                data = Decompress(data);
            }

            var document = JsonConvert.DeserializeObject<TDocument>(Encoding.UTF8.GetString(data), _jsonSettings);

            return Task.FromResult(document);
        }

        private static readonly byte[] _gZipHeaderBytes = { 0x1f, 0x8b };

        internal static bool IsCompressed(byte[] data)
        {
            if (data.Length < _gZipHeaderBytes.Length)
            {
                return false;
            }

            for (var i = 0; i < _gZipHeaderBytes.Length; i++)
            {
                if (data[i] != _gZipHeaderBytes[i])
                {
                    return false;
                }
            }

            return true;
        }

        internal static byte[] Compress(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                input.CopyTo(gzip);
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

            return output.ToArray();
        }
    }
}
