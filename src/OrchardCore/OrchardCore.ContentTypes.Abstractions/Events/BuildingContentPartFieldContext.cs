using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public class BuildingContentPartFieldContext
{
    public readonly string Name;

    public ContentPartFieldDefinitionRecord Record { get; set; }

    public BuildingContentPartFieldContext(string name, ContentPartFieldDefinitionRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
        Record = record;
    }
}

