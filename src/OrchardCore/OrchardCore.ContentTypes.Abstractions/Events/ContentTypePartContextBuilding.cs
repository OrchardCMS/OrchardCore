using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class ContentTypePartContextBuilding : ContentDefinitionBuildingContextBase
{
    public ContentTypePartDefinitionRecord Record { get; set; }

    public ContentTypePartContextBuilding(string name, ContentTypePartDefinitionRecord record)
        : base(name)
    {
        Record = record;
    }
}
