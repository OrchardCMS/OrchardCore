using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Core.Models;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

public sealed class SlidingWindowRateLimiterSource : RateLimiterSourceBase<SlidingWindowRateLimiterData>
{
    public const string SourceName = "SlidingWindow";

    public SlidingWindowRateLimiterSource(IStringLocalizer<SlidingWindowRateLimiterSource> stringLocalizer)
        : base(stringLocalizer)
    {
    }

    public override string Name => SourceName;

    public override LocalizedString DisplayName => S["Sliding window"];

    public override LocalizedString Description => S["Limits requests per IP across overlapping time segments."];

    protected override RateLimitPartition<string> CreatePartition(string policyName, HttpContext context, SlidingWindowRateLimiterData model)
    {
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: $"{policyName}:{SourceName}:{RateLimitPartitionHelpers.GetRemoteIpAddress(context)}",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = model.PermitLimit,
                QueueLimit = model.QueueLimit,
                Window = TimeSpan.FromSeconds(model.WindowSeconds),
                SegmentsPerWindow = model.SegmentsPerWindow,
                AutoReplenishment = true,
            });
    }

    protected override string Describe(SlidingWindowRateLimiterData model)
    {
        return S["{0} requests every {1} seconds across {2} segments, queue limit {3}", model.PermitLimit, model.WindowSeconds, model.SegmentsPerWindow, model.QueueLimit];
    }
}
