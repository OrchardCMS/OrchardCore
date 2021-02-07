using System;
using System.Threading.Tasks;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A generic service to keep in sync any single <see cref="IDocument"/> between a document store and a shared cache.
    /// </summary>
    public interface IDocumentManager<TDocument> where TDocument : class, IDocument, new()
    {
        /// <summary>
        /// Loads a single document from the store (or create a new one) for updating and that should not be cached.
        /// </summary>
        Task<TDocument> GetOrCreateMutableAsync(Func<Task<TDocument>> factoryAsync = null);

        /// <summary>
        /// Gets a single document from the cache (or create a new one) for sharing and that should not be updated.
        /// </summary>
        Task<TDocument> GetOrCreateImmutableAsync(Func<Task<TDocument>> factoryAsync = null);

        /// <summary>
        /// Updates the store with the provided document and then updates the cache after the session is committed.
        /// </summary>
        Task UpdateAsync(TDocument document, Func<TDocument, Task> afterUpdateAsync = null);
    }
}
