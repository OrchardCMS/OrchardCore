using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace OrchardCore.UrlRewriting.Rules;

internal sealed class ExcludeAdminUrlPrefixRule : IRule
{
    private readonly PathString _adminUrlPrefix;

    public ExcludeAdminUrlPrefixRule(string adminUrlPrefix)
    {
        _adminUrlPrefix = new PathString("/" + adminUrlPrefix);
    }

    public void ApplyRule(RewriteContext context)
    {
        if (context.HttpContext.Request.Path.StartsWithSegments(_adminUrlPrefix))
        {
            context.Result = RuleResult.SkipRemainingRules;
        }
        else
        {
            context.Result = RuleResult.ContinueRules;
        }
    }
}
