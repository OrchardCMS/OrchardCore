using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes.ViewModels;

/// <summary>
/// The model of the list of the fields of a content part, in the editor of the content part.
/// </summary>
public class ContentPartFieldsViewModel
{
    /// <summary>
    /// Gets or sets the content part whose fields are listed.
    /// </summary>
    [BindNever]
    public ContentPartDefinition PartDefinition { get; set; }
}
