using OrchardCore.Data.Documents;

namespace OrchardCore.RateLimits.Models;

public sealed class RateLimitPolicyDocument : Document
{
    public List<RateLimitPolicyEntry> Policies { get; init; } = [];
}
