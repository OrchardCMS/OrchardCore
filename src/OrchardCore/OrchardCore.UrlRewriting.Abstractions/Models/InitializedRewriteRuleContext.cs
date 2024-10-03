namespace OrchardCore.UrlRewriting.Models;

public class InitializedRewriteRuleContext
{
    public RewriteRule Rule { get; }

    public InitializedRewriteRuleContext(RewriteRule rule)
    {
        Rule = rule;
    }
}
