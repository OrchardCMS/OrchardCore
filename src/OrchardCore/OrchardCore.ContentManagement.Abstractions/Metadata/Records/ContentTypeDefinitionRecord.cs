using System.Text.Json.Nodes;

namespace OrchardCore.ContentManagement.Metadata.Records;

public class ContentTypeDefinitionRecord
{
    public ContentTypeDefinitionRecord()
    {
        ContentTypePartDefinitionRecords = [];
    }

    public string Name { get; set; }

    public string DisplayName { get; set; }

    public JsonObject Settings { get; set; }

    public IList<ContentTypePartDefinitionRecord> ContentTypePartDefinitionRecords { get; set; }
}
