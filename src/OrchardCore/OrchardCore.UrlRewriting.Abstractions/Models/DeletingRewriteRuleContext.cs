namespace OrchardCore.UrlRewriting.Models;

public sealed class DeletingRewriteRuleContext : RewriteRuleContextBase
{
    public DeletingRewriteRuleContext(RewriteRule rule)
        : base(rule)
    {
    }
}
