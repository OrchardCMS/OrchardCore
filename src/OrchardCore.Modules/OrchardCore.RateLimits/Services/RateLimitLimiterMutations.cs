using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

internal sealed class RateLimitLimiterMutations
{
    private readonly IRateLimitPolicyStore _policies;

    public RateLimitLimiterMutations(IRateLimitPolicyStore policies)
    {
        _policies = policies;
    }

    public static bool CanModify(RateLimitPolicy policy) => policy is not null && !policy.IsEnabled;

    public async ValueTask SaveAsync(RateLimitPolicy policy, RateLimitLimiter limiter)
    {
        if (!CanModify(policy))
        {
            throw new InvalidOperationException("Disable the policy before changing its limiters.");
        }

        var index = policy.Limiters.FindIndex(item => item.Id == limiter.Id);
        if (index < 0)
        {
            policy.Limiters.Add(limiter);
        }
        else
        {
            policy.Limiters[index] = limiter;
        }
        await _policies.UpdateAsync(policy);
    }

    public async ValueTask DeleteAsync(RateLimitPolicy policy, string limiterId)
    {
        if (!CanModify(policy))
        {
            throw new InvalidOperationException("Disable the policy before changing its limiters.");
        }
        if (policy.Limiters.RemoveAll(item => item.Id == limiterId) > 0)
        {
            await _policies.UpdateAsync(policy);
        }
    }
}
