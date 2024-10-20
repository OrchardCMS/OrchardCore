using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public class BuildingContentTypeContext
{
    public readonly string Name;

    public ContentTypeDefinitionRecord Record { get; set; }

    public BuildingContentTypeContext(string name, ContentTypeDefinitionRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
        Record = record;
    }
}
