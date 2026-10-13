using Microsoft.Extensions.Options;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits;

/// <summary>
/// Configures the <see cref="RateLimitPoliciesOptions"/> from the enabled policies of the policy store.
/// </summary>
public sealed class RateLimitPoliciesOptionsConfiguration : IConfigureOptions<RateLimitPoliciesOptions>
{
    private readonly IRateLimitPolicyStore _policyStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitPoliciesOptionsConfiguration"/> class.
    /// </summary>
    /// <param name="policyStore">The policy store that provides enabled policies.</param>
    public RateLimitPoliciesOptionsConfiguration(IRateLimitPolicyStore policyStore)
    {
        _policyStore = policyStore;
    }

    /// <summary>
    /// Configures the enabled policies.
    /// </summary>
    /// <param name="options">The policies options to configure.</param>
    public void Configure(RateLimitPoliciesOptions options)
    {
        options.EnabledPolicies = _policyStore.GetAllAsync(PolicyVersion.Enabled).AsTask().GetAwaiter().GetResult();
    }
}
