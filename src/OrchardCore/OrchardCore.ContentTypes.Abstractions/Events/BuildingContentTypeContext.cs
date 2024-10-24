using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class BuildingContentTypeContext : ContentDefinitionBuildingContextBase
{
    public ContentTypeDefinitionRecord Record { get; set; }

    public BuildingContentTypeContext(string name, ContentTypeDefinitionRecord record)
         : base(name)
    {
        Record = record;
    }
}
