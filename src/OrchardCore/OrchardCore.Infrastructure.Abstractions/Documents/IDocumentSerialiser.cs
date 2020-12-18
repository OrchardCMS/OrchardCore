using System;
using System.Threading.Tasks;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// Serializes and deserializes an <see cref="IDocument"/> into and from a sequence of bytes.
    /// </summary>
    public interface IDocumentSerialiser
    {
        /// <summary>
        /// Serializes an <see cref="IDocument"/> into a sequence of bytes.
        /// </summary>
        Task<byte[]> SerializeAsync<TDocument>(TDocument document, int compressThreshold = Int32.MaxValue) where TDocument : class, IDocument, new();

        /// <summary>
        /// Deserializes an <see cref="IDocument"/> from a sequence of bytes.
        /// </summary>
        Task<TDocument> DeserializeAsync<TDocument>(byte[] data) where TDocument : class, IDocument, new();
    }
}
