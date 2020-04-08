using System.Threading.Tasks;

namespace OrchardCore.Documents
{
    /// <summary>
    /// Shares tenant level properties that are cached and persisted.
    /// </summary>
    public interface IPersistentPropertiesService
    {
        /// <summary>
        /// Gets a persistent property of a given type.
        /// </summary>
        Task<T> GetAsync<T>(string name) where T : new();

        /// <summary>
        /// Sets a persistent property of a given type.
        /// </summary>
        Task SetAsync<T>(string name, T value) where T : new();

        /// <summary>
        /// Removes a persistent property.
        /// </summary>
        Task RemoveAsync(string name);
    }
}
