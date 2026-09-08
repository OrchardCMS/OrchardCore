using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.Html.ViewModels;

public abstract class HtmlViewModelBase
{
    public string Html { get; set; }

    [BindNever]
    public ContentItem ContentItem { get; set; }    
}
