using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.UrlRewriting.ViewModels;

public class RewriteRuleViewModel
{
    public string Name { get; set; }

    public string Pattern { get; set; }

    public bool IgnoreCase { get; set; }

    public RuleAction RuleAction { get; set; }

    public RewriteActionViewModel RewriteAction { get; set; } = new RewriteActionViewModel();

    public RedirectActionViewModel RedirectAction { get; set; } = new RedirectActionViewModel();

    [BindNever]
    public string Substitution => RuleAction == RuleAction.Rewrite ? RewriteAction.RewriteUrl : RedirectAction.RedirectUrl;

    [BindNever]
    public bool IsNew { get; set; }

    [BindNever]
    public List<SelectListItem> AvailableActions { get; set; } = [];
}

public enum RuleAction
{
    Rewrite,
    Redirect
}

public enum RedirectType
{
    MovedPermanently301,
    Found302,
    TemporaryRedirect307,
    PernamentRedirect308
}

public class RedirectActionViewModel
{
    public string RedirectUrl { get; set; }

    public bool AppendQueryString { get; set; } = true;

    public RedirectType RedirectType { get; set; } = RedirectType.Found302;

    [BindNever]
    public List<SelectListItem> AvailableRedirectTypes { get; set; } = [];
}

public class RewriteActionViewModel
{
    public string RewriteUrl { get; set; }

    public bool AppendQueryString { get; set; } = true;

    public bool SkipFurtherRules { get; set; }
}
