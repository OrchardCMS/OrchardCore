using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentManagement
{
    public interface IContentDefinitionStore
    {
        /// <summary>
        /// Loads the content definition from the store for updating and that should not be cached.
        /// </summary>
        Task<ContentDefinitionRecord> LoadContentDefinitionAsync();

        /// <summary>
        /// Gets the content definition from the cache for sharing and that should not be updated.
        /// </summary>
        Task<ContentDefinitionRecord> GetContentDefinitionAsync();

        /// <summary>
        /// Updates the store with the provided content definition and then updates the cache.
        /// </summary>
        Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord);
    }
}
