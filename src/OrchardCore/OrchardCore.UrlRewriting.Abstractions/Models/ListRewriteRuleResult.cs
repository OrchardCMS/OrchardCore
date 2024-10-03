namespace OrchardCore.UrlRewriting.Models;

public class ListRewriteRuleResult
{
    public IEnumerable<RewriteRule> Records { get; set; }

    public int Count { get; set; }
}
