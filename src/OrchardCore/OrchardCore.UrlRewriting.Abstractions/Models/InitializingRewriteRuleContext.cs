namespace OrchardCore.UrlRewriting.Models;

public sealed class InitializingRewriteRuleContext : RewriteRuleContextBase
{
    public InitializingRewriteRuleContext(RewriteRule rule)
        : base(rule)
    {
    }
}
