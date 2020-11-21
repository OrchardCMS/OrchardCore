using System;
using System.Threading.Tasks;
using OrchardCore.Data.Documents;

namespace OrchardCore.Documents
{
    /// <summary>
    /// An <see cref="IDocumentManager{TDocument}"/> using a shared cache but without any persistent storage.
    /// </summary>
    public interface IVolatileDocumentManager<TDocument> : IDocumentManager<TDocument> where TDocument : class, IDocument, new()
    {
        /// <summary>
        /// Executes the provided delegates around updating the cache, the whole being done after the session is committed.
        /// </summary>
        Task UpdateAsync(Func<Task<TDocument>> updatingAsync, Func<TDocument, Task> updatedAsync = null);

        /// <summary>
        /// Executes the provided delegates around updating the cache, the whole being done atomically and after the session is committed.
        /// </summary>
        Task UpdateAtomicAsync(Func<Task<TDocument>> updatingAsync, Func<TDocument, Task> updatedAsync = null);
    }
}
