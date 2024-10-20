using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class BuildingContentPartFieldContext : BuildingContentTypeContextBase
{
    public ContentPartFieldDefinitionRecord Record { get; set; }

    public BuildingContentPartFieldContext(string name, ContentPartFieldDefinitionRecord record)
        : base(name)
    {
        Record = record;
    }
}

