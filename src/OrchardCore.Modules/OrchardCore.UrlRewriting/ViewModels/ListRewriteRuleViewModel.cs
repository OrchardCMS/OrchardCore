namespace OrchardCore.UrlRewriting.ViewModels;

public class ListRewriteRuleViewModel
{
    public IList<RewriteRuleEntry> Rules { get; set; }

    public RewriteRuleOptions Options { get; set; }

    public IEnumerable<string> SourceNames { get; set; }

    public dynamic Pager { get; set; }
}
