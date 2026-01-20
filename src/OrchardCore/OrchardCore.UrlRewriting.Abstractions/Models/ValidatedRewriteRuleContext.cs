namespace OrchardCore.UrlRewriting.Models;

public sealed class ValidatedRewriteRuleContext : RewriteRuleContextBase
{
    public readonly RewriteValidateResult Result;

    public ValidatedRewriteRuleContext(RewriteRule rule, RewriteValidateResult result)
        : base(rule)
    {
        Result = result ?? new();
    }
}
