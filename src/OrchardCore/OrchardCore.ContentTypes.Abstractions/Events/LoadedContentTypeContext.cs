using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public class LoadedContentTypeContext
{
    public ContentTypeDefinitionRecord Record { get; }

    public LoadedContentTypeContext(ContentTypeDefinitionRecord record)
    {
        Record = record;
    }
}
