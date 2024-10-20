using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public class BuildingContentPartDefinitionContext
{
    public readonly string Name;

    public ContentPartDefinitionRecord Record { get; set; }

    public BuildingContentPartDefinitionContext(string name, ContentPartDefinitionRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
        Record = record;
    }
}

