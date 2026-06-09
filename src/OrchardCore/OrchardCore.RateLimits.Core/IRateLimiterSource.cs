using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Core;

public interface IRateLimiterSource
{
    string Name { get; }

    LocalizedString DisplayName { get; }

    LocalizedString Description { get; }

    RateLimitPartition<string> CreatePartition(string policyName, HttpContext context, RateLimitLimiter limiter);

    string Describe(RateLimitLimiter limiter);
}
