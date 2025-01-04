using System.Text.Json.Nodes;

namespace OrchardCore.ContentManagement.Metadata.Models;

public class ContentPartFieldDefinition : ContentDefinition
{
    public ContentPartFieldDefinition(ContentFieldDefinition contentFieldDefinition, string name, JsonObject settings)
    {
        Name = name;
        FieldDefinition = contentFieldDefinition;
        Settings = settings;
    }

    public ContentFieldDefinition FieldDefinition { get; private set; }
    public ContentPartDefinition PartDefinition { get; set; }
    public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
}
