using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class BuildingContentPartDefinitionContext : BuildingContentTypeContextBase
{
    public ContentPartDefinitionRecord Record { get; set; }

    public BuildingContentPartDefinitionContext(string name, ContentPartDefinitionRecord record)
        : base(name)
    {
        Record = record;
    }
}

