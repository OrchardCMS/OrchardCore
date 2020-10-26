using OrchardCore.Documents.Options;

namespace OrchardCore.Documents
{
    /// <summary>
    /// Marker interface used by the <see cref="IDocumentManager{TDocument}"/> indicating
    /// that the document should be compressed before being set in the distributed cache,
    /// unless the <see cref="DocumentOptions.CacheCompression"/> config option is false.
    /// </summary>
    public interface ICompressibleDocument
    {
    }
}
