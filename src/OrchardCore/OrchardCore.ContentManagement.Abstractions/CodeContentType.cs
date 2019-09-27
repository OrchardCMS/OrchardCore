using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentManagement
{
    public abstract class CodeContentType
    {
        public ContentTypeDefinitionRecord ContentTypeDefinitionRecord { get; } = new ContentTypeDefinitionRecord();
    }
}