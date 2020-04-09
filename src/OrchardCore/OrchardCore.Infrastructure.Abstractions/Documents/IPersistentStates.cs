using System.Threading.Tasks;

namespace OrchardCore.Documents
{
    /// <summary>
    /// Shares tenant level states that are cached and persisted.
    /// </summary>
    public interface IPersistentStates
    {
        /// <summary>
        /// Gets a persistent state of a given type.
        /// </summary>
        Task<T> GetAsync<T>(string name) where T : new();

        /// <summary>
        /// Sets a persistent state of a given type.
        /// </summary>
        Task SetAsync<T>(string name, T value) where T : new();

        /// <summary>
        /// Removes a persistent state.
        /// </summary>
        Task RemoveAsync(string name);
    }
}
