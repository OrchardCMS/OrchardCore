using OrchardCore.Environment.Shell;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

internal sealed class RateLimitPolicyMutations
{
    private readonly IRateLimitPolicyStore _policies;
    private readonly IShellReleaseManager _shellRelease;

    public RateLimitPolicyMutations(IRateLimitPolicyStore policies, IShellReleaseManager shellRelease)
    {
        _policies = policies;
        _shellRelease = shellRelease;
    }

    public async ValueTask CreateAsync(RateLimitPolicy policy)
    {
        await _policies.CreateAsync(policy);
        if (policy.IsEnabled)
        {
            _shellRelease.RequestRelease();
        }
    }

    public async ValueTask UpdateAsync(RateLimitPolicy policy, bool wasEnabled)
    {
        await _policies.UpdateAsync(policy);
        if (wasEnabled != policy.IsEnabled)
        {
            _shellRelease.RequestRelease();
        }
    }

    public async ValueTask<int> SetStatusAsync(IEnumerable<RateLimitPolicy> policies, bool enabled)
    {
        var changed = 0;
        foreach (var policy in policies.Where(policy => policy.IsEnabled != enabled))
        {
            if (await _policies.SetStatusAsync(policy.PolicyId, enabled))
            {
                changed++;
            }
        }

        if (changed > 0)
        {
            _shellRelease.RequestRelease();
        }

        return changed;
    }

    public async ValueTask<int> DeleteAsync(IEnumerable<RateLimitPolicy> policies)
    {
        var deleted = 0;
        var reload = false;
        foreach (var policy in policies)
        {
            if (await _policies.DeleteAsync(policy))
            {
                deleted++;
                reload |= policy.IsEnabled;
            }
        }

        if (reload)
        {
            _shellRelease.RequestRelease();
        }

        return deleted;
    }
}
