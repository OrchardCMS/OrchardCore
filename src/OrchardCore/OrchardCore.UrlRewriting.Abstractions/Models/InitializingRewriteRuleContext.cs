namespace OrchardCore.UrlRewriting.Models;

public class InitializingRewriteRuleContext
{
    public RewriteRule Rule { get; }

    public InitializingRewriteRuleContext(RewriteRule rule)
    {
        Rule = rule;
    }
}
