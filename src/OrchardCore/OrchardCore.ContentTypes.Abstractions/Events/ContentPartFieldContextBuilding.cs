using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class ContentPartFieldContextBuilding
{
    public readonly string FieldName;

    public ContentPartFieldDefinitionRecord Record { get; set; }

    public ContentPartFieldContextBuilding(string fieldName, ContentPartFieldDefinitionRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);

        FieldName = fieldName;
        Record = record;
    }
}

