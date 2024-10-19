using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public class LoadingContentPartFieldContext
{
    public ContentPartFieldDefinitionRecord Record { get; }

    public LoadingContentPartFieldContext(ContentPartFieldDefinitionRecord record)
    {
        Record = record;
    }
}
