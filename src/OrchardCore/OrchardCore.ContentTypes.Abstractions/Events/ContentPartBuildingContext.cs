using OrchardCore.ContentManagement.Metadata.Records;

namespace OrchardCore.ContentTypes.Events;

public sealed class ContentPartBuildingContext
{
    public readonly string PartName;

    public ContentPartDefinitionRecord Record { get; set; }

    public ContentPartBuildingContext(string partName, ContentPartDefinitionRecord record)
    {
        ArgumentException.ThrowIfNullOrEmpty(partName);

        PartName = partName;
        Record = record;
    }
}

