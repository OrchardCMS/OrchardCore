namespace OrchardCore.UrlRewriting.Models;

public sealed class InitializedRewriteRuleContext : RewriteRuleContextBase
{
    public InitializedRewriteRuleContext(RewriteRule rule)
        : base(rule)
    {
    }
}
