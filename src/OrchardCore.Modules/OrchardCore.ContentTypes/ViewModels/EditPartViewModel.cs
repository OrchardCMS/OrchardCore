using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Utilities;

namespace OrchardCore.ContentTypes.ViewModels;

public class EditPartViewModel
{
    public EditPartViewModel()
    {
    }

    public EditPartViewModel(ContentPartDefinition contentPartDefinition)
    {
        Name = contentPartDefinition.Name;
        PartDefinition = contentPartDefinition;
        _displayName = contentPartDefinition.DisplayName();
    }

    public string Name { get; set; }

    private string _displayName;

    [Required]
    public string DisplayName
    {
        get { return !string.IsNullOrWhiteSpace(_displayName) ? _displayName : Name.TrimEndString("Part").CamelFriendly(); }
        set { _displayName = value; }
    }

    public string Description
    {
        get { return PartDefinition.GetSettings<ContentPartSettings>().Description; }
        set { PartDefinition.GetSettings<ContentPartSettings>().Description = value; }
    }

    [BindNever]
    public ContentPartDefinition PartDefinition { get; private set; }

    [BindNever]
    public dynamic Editor { get; set; }
}
