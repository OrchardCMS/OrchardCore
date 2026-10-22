using OrchardCore.Admin;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.ViewModels;

public class ListRewriteRuleViewModel
{
    /// <summary>
    /// The rewrite rules in pipeline order, before their row shapes are built.
    /// </summary>
    public IList<RewriteRule> Rules { get; set; }

    public RewriteRuleOptions Options { get; set; }

    public IEnumerable<string> SourceNames { get; set; }

    /// <summary>
    /// The data context for rendering the rules with the configured layout.
    /// </summary>
    public AdminListContext List { get; set; }
}
