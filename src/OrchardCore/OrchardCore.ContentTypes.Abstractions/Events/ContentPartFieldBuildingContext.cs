using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class ContentPartFieldBuildingContext
{
    public readonly string FieldName;

    public ContentPartFieldDefinitionRecord Record { get; set; }

    public ContentPartFieldBuildingContext(string fieldName, ContentPartFieldDefinitionRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);

        FieldName = fieldName;
        Record = record;
    }
}

