namespace OrchardCore.UrlRewriting.Models;

public sealed class DeletedRewriteRuleContext : RewriteRuleContextBase
{
    public DeletedRewriteRuleContext(RewriteRule rule)
        : base(rule)
    {
    }
}
