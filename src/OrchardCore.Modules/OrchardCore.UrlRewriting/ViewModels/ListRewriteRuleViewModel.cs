using OrchardCore.DisplayManagement;

namespace OrchardCore.UrlRewriting.ViewModels;

public class ListRewriteRuleViewModel
{
    public IList<RewriteRuleEntry> Rules { get; set; }

    public RewriteRuleOptions Options { get; set; }

    public IEnumerable<string> SourceNames { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the rules with the configured layout.
    /// </summary>
    public IShape List { get; set; }
}
