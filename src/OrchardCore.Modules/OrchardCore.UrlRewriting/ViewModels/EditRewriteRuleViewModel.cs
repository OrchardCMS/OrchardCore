using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.ViewModels;

public class EditRewriteRuleViewModel
{
    public string Name { get; set; }

    public string Source { get; set; }

    public bool SkipFurtherRules { get; set; }

    [BindNever]
    public RewriteRule Rule { get; set; }
}
