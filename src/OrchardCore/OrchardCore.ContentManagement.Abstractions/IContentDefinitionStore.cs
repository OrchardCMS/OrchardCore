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
        /// Gets a single document (or create a new one) for caching and that should not be updated.
        /// </summary>
        Task<ContentDefinitionRecord> GetContentDefinitionAsync();

        Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord);
    }
}
