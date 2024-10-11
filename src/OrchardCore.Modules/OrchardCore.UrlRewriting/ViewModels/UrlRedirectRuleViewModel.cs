using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.ViewModels;

public class UrlRedirectRuleViewModel
{
    public string SubstitutionUrl { get; set; }

    public bool AppendQueryString { get; set; }

    public string Pattern { get; set; }

    public bool IgnoreCase { get; set; }

    public RedirectType RedirectType { get; set; }

    [BindNever]
    public List<SelectListItem> RedirectTypes { get; set; }
}
