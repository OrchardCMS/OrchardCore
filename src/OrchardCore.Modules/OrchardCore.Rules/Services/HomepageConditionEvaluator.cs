using Microsoft.AspNetCore.Http;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services;

public class HomepageConditionEvaluator : ConditionEvaluator<HomepageCondition>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HomepageConditionEvaluator(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<bool> EvaluateAsync(HomepageCondition condition)
    {
        var requestPath = _httpContextAccessor.HttpContext.Request.Path.Value;

        var result = string.Equals("/", requestPath, StringComparison.Ordinal) || string.IsNullOrEmpty(requestPath);

        if (!condition.Value)
        {
            result = !result;
        }

        return result ? True : False;
    }
}
