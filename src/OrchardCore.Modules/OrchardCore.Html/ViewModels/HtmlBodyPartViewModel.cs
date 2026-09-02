using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Html.Models;

namespace OrchardCore.Html.ViewModels;

public class HtmlBodyPartViewModel : HtmlViewModelBase
{
    [BindNever]
    public HtmlBodyPart HtmlBodyPart { get; set; }

    [BindNever]
    public ContentTypePartDefinition TypePartDefinition { get; set; }
}
