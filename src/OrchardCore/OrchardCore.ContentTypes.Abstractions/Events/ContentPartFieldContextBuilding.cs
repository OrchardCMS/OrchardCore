using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class ContentPartFieldContextBuilding : ContentDefinitionBuildingContextBase
{
    public ContentPartFieldDefinitionRecord Record { get; set; }

    public ContentPartFieldContextBuilding(string name, ContentPartFieldDefinitionRecord record)
        : base(name)
    {
        Record = record;
    }
}

