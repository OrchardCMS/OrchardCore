using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class BuildingContentTypePartContext : BuildingContentTypeContextBase
{
    public ContentTypePartDefinitionRecord Record { get; set; }

    public BuildingContentTypePartContext(string name, ContentTypePartDefinitionRecord record)
        : base(name)
    {
        Record = record;
    }
}
