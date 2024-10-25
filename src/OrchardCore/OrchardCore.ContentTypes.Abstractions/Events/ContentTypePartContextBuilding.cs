using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class ContentTypePartContextBuilding
{
    public readonly string PartName;

    public ContentTypePartDefinitionRecord Record { get; set; }

    public ContentTypePartContextBuilding(string partName, ContentTypePartDefinitionRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(partName);

        PartName = partName;
        Record = record;
    }
}
