using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentManagement
{
    public interface IContentDefinitionStore
    {
        /// <summary>
        /// Loads a single document (or create a new one) for updating and that should not be cached.
        /// </summary>
        Task<ContentDefinitionRecord> LoadContentDefinitionAsync();

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated,
        /// and specifies if the returned document can be cached or not if it has been already loaded.
        /// </summary>
        Task<(bool, ContentDefinitionRecord)> GetContentDefinitionAsync();

        Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord);
    }
}
