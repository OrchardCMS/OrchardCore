using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Core;

public interface IRateLimitPolicyStore
{
    Task<RateLimitPolicyDocument> LoadAsync();

    Task<RateLimitPolicyDocument> GetAsync();

    Task SaveAsync(RateLimitPolicyDocument document);

    Task<IEnumerable<RateLimitPolicy>> GetPublishedPoliciesAsync();

    Task<RateLimitPolicyEntry> FindAsync(string policyId);

    Task SaveDraftAsync(string policyId, RateLimitPolicy draft);

    Task DeleteAsync(string policyId);

    Task PublishAsync(IEnumerable<string> policyIds);
}
