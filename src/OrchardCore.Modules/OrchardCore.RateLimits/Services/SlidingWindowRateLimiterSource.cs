using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Core.Models;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

/// <summary>
/// Creates sliding-window rate-limit partitions for policy limiters.
/// </summary>
public sealed class SlidingWindowRateLimiterSource : RateLimiterSourceBase<SlidingWindowRateLimiterData>
{
    /// <summary>
    /// The persisted source name for sliding-window limiters.
    /// </summary>
    public const string SourceName = "SlidingWindow";

    /// <summary>
    /// Initializes a new instance of the <see cref="SlidingWindowRateLimiterSource"/> class.
    /// </summary>
    /// <param name="stringLocalizer">The string localizer used by the source.</param>
    public SlidingWindowRateLimiterSource(IStringLocalizer<SlidingWindowRateLimiterSource> stringLocalizer)
        : base(stringLocalizer)
    {
    }

    /// <inheritdoc />
    public override string Name => SourceName;

    /// <inheritdoc />
    public override LocalizedString DisplayName => S["Sliding window"];

    /// <inheritdoc />
    public override LocalizedString Description => S["Spreads a request limit across a rolling time period so traffic is smoothed instead of resetting all at once."];

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
