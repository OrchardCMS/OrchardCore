using System;
using System.Threading.Tasks;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// Serializes and deserializes <see cref="IDocument"/> into and from a sequence of bytes.
    /// </summary>
    public interface IDocumentSerialiser<TDocument> where TDocument : class, IDocument, new()
    {
        /// <summary>
        /// Serializes en <see cref="IDocument"/> into a sequence of bytes.
        /// </summary>
        Task<byte[]> SerializeAsync(TDocument document, int compressThreshold = Int32.MaxValue);

        /// <summary>
        /// Deserializes an <see cref="IDocument"/> from a sequence of bytes.
        /// </summary>
        Task<TDocument> DeserializeAsync(byte[] data);
    }
}
