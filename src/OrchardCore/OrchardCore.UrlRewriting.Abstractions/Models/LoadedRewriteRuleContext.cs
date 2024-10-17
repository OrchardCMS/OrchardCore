namespace OrchardCore.UrlRewriting.Models;

public sealed class LoadedRewriteRuleContext : RewriteRuleContextBase
{
    public LoadedRewriteRuleContext(RewriteRule rule)
        : base(rule)
    {
    }
}
