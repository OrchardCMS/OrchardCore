using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public class LoadingContentTypePartContext
{
    public ContentTypePartDefinitionRecord Record { get; }

    public LoadingContentTypePartContext(ContentTypePartDefinitionRecord record)
    {
        Record = record;
    }
}
