using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Data;

namespace OrchardCore.Infrastructure.Cache
{
    /// <summary>
    /// A generic service to keep in sync a multi level distributed cache with a given document store.
    /// </summary>
    public interface IDocumentStoreDistributedCache<TDocumentStore> : IDocumentStoreDistributedCache where TDocumentStore : ICacheableDocumentStore
    {
    }

    /// <summary>
    /// Keeps in sync a multi level distributed cache with a given document store.
    /// </summary>
    public interface IDocumentStoreDistributedCache
    {
        /// <summary>
        /// Loads a single document from the store or create a new one by using <see cref="ICacheableDocumentStore.LoadForUpdateAsync"/>.
        /// </summary>
        Task<T> LoadAsync<T>() where T : DistributedDocument, new();

        /// <summary>
        /// Gets a single document from the cache or create a new one by using <see cref="ICacheableDocumentStore.GetForCachingAsync"/>.
        /// </summary>
        Task<T> GetAsync<T>(Func<T> factory = null, DistributedCacheEntryOptions options = null, bool checkConsistency = true) where T : DistributedDocument, new();

        /// <summary>
        /// Updates the store with the provided document and then updates the cache.
        /// </summary>
        Task UpdateAsync<T>(T document, DistributedCacheEntryOptions options = null, bool checkConcurrency = true, bool checkConsistency = true) where T : DistributedDocument, new();
    }
}
