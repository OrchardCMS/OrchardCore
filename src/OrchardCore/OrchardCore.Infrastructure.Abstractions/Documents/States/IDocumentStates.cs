using System.Threading.Tasks;

namespace OrchardCore.Documents.States
{
    /// <summary>
    /// Shares tenant level states.
    /// </summary>
    public interface IDocumentStates
    {
        /// <summary>
        /// Gets a persistent state of a given type.
        /// </summary>
        Task<T> GetAsync<T>(string name) where T : new();

        /// <summary>
        /// Sets a persistent state once the ambient session is committed.
        /// </summary>
        Task SetAsync<T>(string name, T value) where T : new();

        /// <summary>
        /// Removes a persistent state once the ambient session is committed.
        /// </summary>
        Task RemoveAsync(string name);
    }
}
