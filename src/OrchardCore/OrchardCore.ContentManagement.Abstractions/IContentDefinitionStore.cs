using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Documents;

namespace OrchardCore.ContentManagement
{
    public interface IContentDefinitionStore
    {
        /// <summary>
        /// Loads a single document (or create a new one) for updating and that should not be cached.
        /// </summary>
        Task<ContentDefinitionDocument> LoadContentDefinitionAsync();

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated.
        /// </summary>
        Task<ContentDefinitionDocument> GetContentDefinitionAsync();

        Task SaveContentDefinitionAsync(ContentDefinitionDocument contentDefinitionDocument);
    }
}
