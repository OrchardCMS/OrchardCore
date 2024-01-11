using System.Threading.Tasks;

namespace OrchardCore.Documents
{
    /// <summary>
    /// A generic service to keep in sync any single <see cref="IDocumentEntity"/> between a document store and a shared cache.
    /// </summary>
    public interface IDocumentEntityManager<TDocumentEntity> where TDocumentEntity : class, IDocumentEntity, new()
    {
        /// <summary>
        /// Gets a persistent property of a given type.
        /// </summary>
        Task<T> GetAsync<T>(string name) where T : new();

        /// <summary>
        /// Sets a persistent property once the ambient session is committed.
        /// </summary>
        Task SetAsync<T>(string name, T value) where T : new();

        /// <summary>
        /// Removes a persistent property once the ambient session is committed.
        /// </summary>
        Task RemoveAsync(string name);
    }
}
