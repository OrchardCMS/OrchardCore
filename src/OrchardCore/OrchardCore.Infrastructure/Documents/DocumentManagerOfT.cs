using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Data.Documents;
using OrchardCore.Documents.Options;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A <see cref="DocumentManager{TDocument}"/> using a given type of <see cref="IDocumentStore"/>.
    /// </summary>
    public class DocumentManager<TDocumentStore, TDocument> : DocumentManager<TDocument>, IDocumentManager<TDocumentStore, TDocument>
        where TDocumentStore : IDocumentStore where TDocument : class, IDocument, new()
    {
        public DocumentManager(
            TDocumentStore documentStore,
            IDistributedCache distributedCache,
            IMemoryCache memoryCache,
            DocumentOptions<TDocument> options)
            : base(documentStore, distributedCache, memoryCache, options)
        {
        }
    }
}
