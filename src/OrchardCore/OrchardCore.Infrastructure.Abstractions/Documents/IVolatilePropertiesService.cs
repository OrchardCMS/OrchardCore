using System.Threading.Tasks;

namespace OrchardCore.Documents
{
    /// <summary>
    /// Shares tenant level properties that are cached but not persisted.
    /// </summary>
    public interface IVolatilePropertiesService
    {
        /// <summary>
        /// Gets a volatile property of a given type.
        /// </summary>
        Task<T> GetAsync<T>(string name) where T : new();

        /// <summary>
        /// Sets a volatile property of a given type.
        /// </summary>
        Task SetAsync<T>(string name, T value) where T : new();

        /// <summary>
        /// Removes a volatile property.
        /// </summary>
        Task RemoveAsync(string name);
    }
}
