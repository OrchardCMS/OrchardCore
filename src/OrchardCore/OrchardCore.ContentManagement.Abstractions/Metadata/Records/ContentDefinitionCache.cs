using Microsoft.Extensions.Primitives;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    public class ContentDefinitionCache
    {
        public ContentDefinitionRecord ContentDefinitionRecord { get; set; }
        public IChangeToken ChangeToken { get; set; }
    }
}
