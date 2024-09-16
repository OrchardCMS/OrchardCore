using System.Text.Json.Nodes;

namespace OrchardCore.ContentManagement.Metadata.Models;

public class ContentTypePartDefinition : ContentDefinition
{
    public ContentTypePartDefinition(string name, ContentPartDefinition contentPartDefinition, JsonObject settings)
    {
        Name = name;
        PartDefinition = contentPartDefinition;
        Settings = settings;

        foreach (var field in PartDefinition.Fields)
        {
            field.ContentTypePartDefinition = this;
        }
    }

    public ContentPartDefinition PartDefinition { get; private set; }
    public ContentTypeDefinition ContentTypeDefinition { get; set; }
}
