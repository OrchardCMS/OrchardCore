using System;
using System.IO;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using OrchardCore.Documents;

namespace OrchardCore.Autoroute.Services
{
    /// <summary>
    /// An <see cref="IDocumentSerialiser{TDocument}"/> using `MessagePack`.
    /// </summary>
    public class AutorouteDocumentSerializer : IDocumentSerialiser<AutorouteDocument>
    {
        public AutorouteDocumentSerializer()
        {
        }

        public async Task<byte[]> SerializeAsync(AutorouteDocument document, int compressThreshold = Int32.MaxValue)
        {
            byte[] data;

            using (var stream = new MemoryStream())
            {
                await SerializeAsync(stream, document);
                data = stream.ToArray();
            }

            return data;
        }

        public async Task<AutorouteDocument> DeserializeAsync(byte[] data)
        {
            AutorouteDocument document;
            using (var stream = new MemoryStream(data))
            {
                document = await DeserializeAsync(stream);
            }

            return document;
        }

        internal static Task SerializeAsync(Stream stream, AutorouteDocument document)
        {
            var lz4Options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4Block);
            return MessagePackSerializer.SerializeAsync(stream, document, lz4Options);
        }

        internal static ValueTask<AutorouteDocument> DeserializeAsync(Stream stream)
        {
            var lz4Options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4Block);
            return MessagePackSerializer.DeserializeAsync<AutorouteDocument>(stream, lz4Options);
        }
    }
}
