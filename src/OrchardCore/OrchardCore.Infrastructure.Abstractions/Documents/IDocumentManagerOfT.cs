using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// An <see cref="IDocumentManager{TDocument}"/> using a given type of <see cref="IDocumentStore"/>.
    /// </summary>
    public interface IDocumentManager<TDocumentStore, TDocument> : IDocumentManager<TDocument>
        where TDocumentStore : IDocumentStore where TDocument : class, IDocument, new()
    {
    }
}
