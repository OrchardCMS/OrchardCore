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
        /// Executes the provided delegate and updates the cache, the whole being done atomically and after the session is committed,
        /// this only if a lock can be acquired (default timeout to 10s), and atomically if the lock doesn't expire (default to 10s).
        /// </summary>
        Task UpdateAtomicAsync(Func<Task<TDocument>> updateAsync, Func<TDocument, Task> afterUpdateAsync = null);
    }
}
