using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public class LoadedContentPartFieldContext
{
    public ContentPartFieldDefinitionRecord Record { get; }

    public LoadedContentPartFieldContext(ContentPartFieldDefinitionRecord record)
    {
        Record = record;
    }
}
