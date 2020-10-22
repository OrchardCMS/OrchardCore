using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// An <see cref="IDocumentManager{TDocument}"/> using a shared cache but without any persistent storage.
    /// </summary>
    public interface IVolatileDocumentManager<TDocument> : IDocumentManager<TDocument> where TDocument : class, IDocument, new()
    {
    }
}
