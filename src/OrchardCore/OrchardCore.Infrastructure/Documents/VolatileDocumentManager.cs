using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A generic service to manage any single <see cref="IDocument"/> from a shared cache and without any persistent storage.
    /// </summary>
    public class VolatileDocumentManager<TDocument> : DocumentManager<TDocument>, IVolatileDocumentManager<TDocument> where TDocument : class, IDocument, new()
    {
        public VolatileDocumentManager(IDistributedCache distributedCache, IMemoryCache memoryCache, DocumentOptions<TDocument> options)
            : base(documentStore: null, distributedCache, memoryCache, options)
        {
        }
    }
}
