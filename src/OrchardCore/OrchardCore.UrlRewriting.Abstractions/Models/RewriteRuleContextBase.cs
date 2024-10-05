namespace OrchardCore.UrlRewriting.Models;

public abstract class RewriteRuleContextBase
{
    public RewriteRule Rule { get; }

    public RewriteRuleContextBase(RewriteRule rule)
    {
        Rule = rule;
    }
}
