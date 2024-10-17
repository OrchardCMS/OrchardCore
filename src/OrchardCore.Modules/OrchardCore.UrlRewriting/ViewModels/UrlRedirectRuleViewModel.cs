using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.ViewModels;

public class UrlRedirectRuleViewModel
{
    public string Pattern { get; set; }

    public string SubstitutionPattern { get; set; }

    public bool IsCaseInsensitive { get; set; }

    public QueryStringPolicy QueryStringPolicy { get; set; }

    public RedirectType RedirectType { get; set; }

    [BindNever]
    public List<SelectListItem> RedirectTypes { get; set; }

    [BindNever]
    public List<SelectListItem> QueryStringPolicies { get; set; }
}
