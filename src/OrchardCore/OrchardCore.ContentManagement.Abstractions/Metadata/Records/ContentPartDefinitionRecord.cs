using System.Text.Json.Nodes;

namespace OrchardCore.ContentManagement.Metadata.Records;

public class ContentPartDefinitionRecord
{
    public ContentPartDefinitionRecord()
    {
        ContentPartFieldDefinitionRecords = [];
        Settings = [];
    }

    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the settings of a part, like description, or any property that a module would attach
    /// to a part.
    /// </summary>
    public JsonObject Settings { get; set; }

    public IList<ContentPartFieldDefinitionRecord> ContentPartFieldDefinitionRecords { get; set; }
}
