using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public class BuildingContentTypePartContext
{
    public readonly string Name;

    public ContentTypePartDefinitionRecord Record { get; set; }

    public BuildingContentTypePartContext(string name, ContentTypePartDefinitionRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
        Record = record;
    }
}
