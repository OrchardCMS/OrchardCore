using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace OrchardCore.UrlRewriting.Rules;

internal sealed class ExcludeUrlPrefixRule : IRule
{
    private readonly PathString _prefix;

    public ExcludeUrlPrefixRule(PathString prefix)
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        _prefix = prefix;
    }

    public void ApplyRule(RewriteContext context)
    {
        if (context.HttpContext.Request.Path.StartsWithSegments(_prefix))
        {
            context.Result = RuleResult.SkipRemainingRules;

            return;
        }

        context.Result = RuleResult.ContinueRules;
    }
}
