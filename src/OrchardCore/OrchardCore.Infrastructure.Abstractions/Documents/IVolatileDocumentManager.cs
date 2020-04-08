using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A generic service to manage any single <see cref="IDocument"/> from a shared cache and without any persistent storage.
    /// </summary>
    public interface IVolatileDocumentManager<TDocument> : IDocumentManager<TDocument> where TDocument : class, IDocument, new()
    {
    }
}
