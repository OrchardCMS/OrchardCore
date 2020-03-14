using System;
using System.Threading.Tasks;
using OrchardCore.Data;

namespace OrchardCore.Infrastructure.Cache
{
    /// <summary>
    /// A generic service to keep in sync a multi level distributed cache with a given document store.
    /// </summary>
    public interface IDocumentManager<TDocument> where TDocument : DistributedDocument, new()
    {
        /// <summary>
        /// Loads a single document from the store or create a new one by using <see cref="ICacheableDocumentStore.GetMutableAsync"/>.
        /// </summary>
        Task<TDocument> GetMutableAsync();

        /// <summary>
        /// Gets a single document from the cache or create a new one by using <see cref="ICacheableDocumentStore.GetImmutableAsync"/>.
        /// </summary>
        Task<TDocument> GetImmutableAsync(Func<TDocument> factory = null);

        /// <summary>
        /// Updates the store with the provided document and then updates the cache.
        /// </summary>
        Task UpdateAsync(TDocument document, bool checkConcurrency = true);
    }
}
