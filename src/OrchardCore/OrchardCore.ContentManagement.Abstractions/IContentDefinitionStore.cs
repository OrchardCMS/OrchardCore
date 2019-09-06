using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Documents;

namespace OrchardCore.ContentManagement
{
    public interface IContentDefinitionStore
    {
        Task<ContentDefinitionDocument> LoadContentDefinitionAsync();

        Task SaveContentDefinitionAsync(ContentDefinitionDocument contentDefinitionRecord);
    }
}
