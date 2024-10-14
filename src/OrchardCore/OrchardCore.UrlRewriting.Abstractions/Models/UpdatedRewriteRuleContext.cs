namespace OrchardCore.UrlRewriting.Models;

public sealed class UpdatedRewriteRuleContext : RewriteRuleContextBase
{
    public UpdatedRewriteRuleContext(RewriteRule rule)
        : base(rule)
    {
    }
}
