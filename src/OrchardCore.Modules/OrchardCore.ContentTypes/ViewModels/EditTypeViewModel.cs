using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentTypes.ViewModels;

public class EditTypeViewModel
{
    public EditTypeViewModel()
    {
        Settings = [];
    }

    public EditTypeViewModel(ContentTypeDefinition contentTypeDefinition)
    {
        Name = contentTypeDefinition.Name;
        Category = contentTypeDefinition.GetSettings<ContentTypeSettings>().Category;
        DisplayName = contentTypeDefinition.DisplayName;
        Settings = contentTypeDefinition.Settings;
        TypeDefinition = contentTypeDefinition;
    }

    public string Name { get; set; }

    public string Category { get; set; }

    public string DisplayName { get; set; }

    public string[] OrderedFieldNames { get; set; }

    public string[] OrderedPartNames { get; set; }

    [BindNever]
    public JsonObject Settings { get; set; }

    [BindNever]
    public ContentTypeDefinition TypeDefinition { get; set; }

    [BindNever]
    public dynamic Editor { get; set; }
}
