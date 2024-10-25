using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class BuildingContentTypeContext
{
    public readonly string TypeName;

    public ContentTypeDefinitionRecord Record { get; set; }

    public BuildingContentTypeContext(string typeName, ContentTypeDefinitionRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeName);

        TypeName = typeName;
        Record = record;
    }
}
