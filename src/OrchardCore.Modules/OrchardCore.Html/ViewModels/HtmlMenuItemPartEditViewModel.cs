using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Html.Models;

namespace OrchardCore.Html.ViewModels;

public class HtmlMenuItemPartEditViewModel
{
    public string Name { get; set; }

    public string Url { get; set; }

    public string Target { get; set; }

    public string Html { get; set; }

    [BindNever]
    public HtmlMenuItemPart MenuItemPart { get; set; }
}
