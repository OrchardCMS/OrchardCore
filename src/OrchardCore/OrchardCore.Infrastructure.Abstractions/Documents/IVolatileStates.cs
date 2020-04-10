using System.Threading.Tasks;

namespace OrchardCore.Documents
{
    /// <summary>
    /// Shares tenant level states that are cached but not persisted.
    /// </summary>
    public interface IVolatileStates
    {
        /// <summary>
        /// Gets a volatile state of a given type.
        /// </summary>
        Task<T> GetAsync<T>(string key) where T : new();

        /// <summary>
        /// Sets a volatile state once the ambient session is committed.
        /// </summary>
        Task SetAsync<T>(string key, T value) where T : new();

        /// <summary>
        /// Removes a volatile state once the ambient session is committed.
        /// </summary>
        Task RemoveAsync(string key);
    }
}
