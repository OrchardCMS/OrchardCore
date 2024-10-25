using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class ContentPartDefinitionContextBuilding
{
    public readonly string PartName;

    public ContentPartDefinitionRecord Record { get; set; }

    public ContentPartDefinitionContextBuilding(string partName, ContentPartDefinitionRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(partName);

        PartName = partName;
        Record = record;
    }
}

