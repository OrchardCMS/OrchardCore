using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;

namespace OrchardCore.UrlRewriting.Rules;

/// <summary>
/// Applies the rules of the current <see cref="RewriteOptions"/>, so that the rules can be updated
/// without rebuilding the rewrite middleware.
/// </summary>
internal sealed class MonitoredRewriteRule : IRule
{
    private readonly IOptionsMonitor<RewriteOptions> _rewriteOptions;

    public MonitoredRewriteRule(IOptionsMonitor<RewriteOptions> rewriteOptions)
    {
        _rewriteOptions = rewriteOptions;
    }

    public void ApplyRule(RewriteContext context)
    {
        foreach (var rule in _rewriteOptions.CurrentValue.Rules)
        {
            rule.ApplyRule(context);

            // Let the rewrite middleware handle the result, as it would for its own rules.
            if (context.Result != RuleResult.ContinueRules)
            {
                return;
            }
        }
    }
}
