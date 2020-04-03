using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentManagement
{
    public interface IContentDefinitionStore
    {
        Task<ContentDefinitionRecord> LoadContentDefinitionAsync();

        Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord);
    }
}
