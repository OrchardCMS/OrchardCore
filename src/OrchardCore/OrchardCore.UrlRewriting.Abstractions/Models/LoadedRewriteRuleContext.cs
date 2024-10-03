namespace OrchardCore.UrlRewriting.Models;

public class LoadedRewriteRuleContext
{
    public RewriteRule Rule { get; }

    public LoadedRewriteRuleContext(RewriteRule rule)
    {
        Rule = rule;
    }
}
