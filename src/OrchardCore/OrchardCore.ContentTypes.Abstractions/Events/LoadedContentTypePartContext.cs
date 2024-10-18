using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public class LoadedContentTypePartContext
{
    public ContentTypePartDefinitionRecord Record { get; }

    public LoadedContentTypePartContext(ContentTypePartDefinitionRecord record)
    {
        Record = record;
    }
}
