using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.UrlRewriting.ViewModels;

public class RewriteRuleOptions
{
    public string Search { get; set; }

    public RewriteRuleAction BulkAction { get; set; }

    [BindNever]
    public IList<SelectListItem> BulkActions { get; set; }
}
