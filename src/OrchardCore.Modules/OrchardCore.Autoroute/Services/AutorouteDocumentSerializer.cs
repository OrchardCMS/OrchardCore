using System;
using System.IO;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using OrchardCore.Data.Documents;
using OrchardCore.Documents;

namespace OrchardCore.Autoroute.Services
{
    /// <summary>
    /// An <see cref="IDocumentSerialiser"/> using `MessagePack`.
    /// </summary>
    public class AutorouteDocumentSerializer : IDocumentSerialiser
    {
        public static AutorouteDocumentSerializer Instance = new AutorouteDocumentSerializer();

        public AutorouteDocumentSerializer()
        {
        }

        public async Task<byte[]> SerializeAsync<TDocument>(TDocument document, int compressThreshold = Int32.MaxValue) where TDocument : class, IDocument, new()
        {
            using var stream = new MemoryStream();
            await SerializeAsync(stream, document);
            return stream.ToArray();
        }

        public async Task<TDocument> DeserializeAsync<TDocument>(byte[] data) where TDocument : class, IDocument, new()
        {
            using var stream = new MemoryStream(data);
            return await DeserializeAsync<TDocument>(stream);
        }

        internal static Task SerializeAsync<TDocument>(Stream stream, TDocument document)
        {
            var lz4Options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4Block);
            return MessagePackSerializer.SerializeAsync(stream, document, lz4Options);
        }

        internal static ValueTask<TDocument> DeserializeAsync<TDocument>(Stream stream)
        {
            var lz4Options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4Block);
            return MessagePackSerializer.DeserializeAsync<TDocument>(stream, lz4Options);
        }
    }
}
