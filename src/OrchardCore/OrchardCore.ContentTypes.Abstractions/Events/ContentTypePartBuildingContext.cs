using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class ContentTypePartBuildingContext
{
    public readonly string PartName;

    public ContentTypePartDefinitionRecord Record { get; set; }

    public ContentTypePartBuildingContext(string partName, ContentTypePartDefinitionRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(partName);

        PartName = partName;
        Record = record;
    }
}
