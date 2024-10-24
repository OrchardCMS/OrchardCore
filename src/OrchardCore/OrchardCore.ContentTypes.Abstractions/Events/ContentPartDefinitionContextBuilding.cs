using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class ContentPartDefinitionContextBuilding : ContentDefinitionBuildingContextBase
{
    public ContentPartDefinitionRecord Record { get; set; }

    public ContentPartDefinitionContextBuilding(string name, ContentPartDefinitionRecord record)
        : base(name)
    {
        Record = record;
    }
}

