using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public class LoadingContentTypeContext
{
    public ContentTypeDefinitionRecord Record { get; }

    public LoadingContentTypeContext(ContentTypeDefinitionRecord record)
    {
        Record = record;
    }
}
