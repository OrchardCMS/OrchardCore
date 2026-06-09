using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Core.Models;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

public sealed class ConcurrencyRateLimiterSource : RateLimiterSourceBase<ConcurrencyRateLimiterData>
{
    public const string SourceName = "Concurrency";

    public ConcurrencyRateLimiterSource(IStringLocalizer<ConcurrencyRateLimiterSource> stringLocalizer)
        : base(stringLocalizer)
    {
    }

    public override string Name => SourceName;

    public override LocalizedString DisplayName => S["Concurrency"];

    public override LocalizedString Description => S["Limits concurrent in-flight requests per IP."];

    protected override RateLimitPartition<string> CreatePartition(string policyName, HttpContext context, ConcurrencyRateLimiterData model)
    {
        return RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: $"{policyName}:{SourceName}:{RateLimitPartitionHelpers.GetRemoteIpAddress(context)}",
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = model.PermitLimit,
                QueueLimit = model.QueueLimit,
                QueueProcessingOrder = model.QueueProcessingOrder,
            });
    }

    protected override string Describe(ConcurrencyRateLimiterData model)
    {
        return S["{0} concurrent requests, queue limit {1}, queue order {2}", model.PermitLimit, model.QueueLimit, model.QueueProcessingOrder];
    }
}
