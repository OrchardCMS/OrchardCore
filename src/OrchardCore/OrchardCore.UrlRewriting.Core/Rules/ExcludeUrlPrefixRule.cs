using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace OrchardCore.UrlRewriting.Rules;

internal sealed class ExcludeUrlPrefixRule : IRule
{
    private readonly PathString _adminUrlPrefix;

    public ExcludeUrlPrefixRule(string prefix)
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        _adminUrlPrefix = new PathString('/' + prefix.TrimStart('/'));
    }

    public void ApplyRule(RewriteContext context)
    {
        if (context.HttpContext.Request.Path.StartsWithSegments(_adminUrlPrefix))
        {
            context.Result = RuleResult.SkipRemainingRules;

            return;
        }

        context.Result = RuleResult.ContinueRules;
    }
}
