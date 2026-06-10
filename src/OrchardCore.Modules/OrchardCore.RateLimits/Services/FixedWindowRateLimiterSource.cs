using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Core.Models;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

/// <summary>
/// Creates fixed-window rate-limit partitions for policy limiters.
/// </summary>
public sealed class FixedWindowRateLimiterSource : RateLimiterSourceBase<FixedWindowRateLimiterData>
{
    /// <summary>
    /// The persisted source name for fixed-window limiters.
    /// </summary>
    public const string SourceName = "FixedWindow";

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedWindowRateLimiterSource"/> class.
    /// </summary>
    /// <param name="stringLocalizer">The string localizer used by the source.</param>
    public FixedWindowRateLimiterSource(IStringLocalizer<FixedWindowRateLimiterSource> stringLocalizer)
        : base(stringLocalizer)
    {
    }

    /// <inheritdoc />
    public override string Name => SourceName;

    /// <inheritdoc />
    public override LocalizedString DisplayName => S["Fixed window"];

    /// <inheritdoc />
    public override LocalizedString Description => S["Limits requests per IP across a fixed time window."];

    protected override RateLimitPartition<string> CreatePartition(string policyName, HttpContext context, FixedWindowRateLimiterData model)
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{policyName}:{SourceName}:{RateLimitPartitionHelpers.GetRemoteIpAddress(context)}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = model.PermitLimit,
                QueueLimit = model.QueueLimit,
                Window = TimeSpan.FromSeconds(model.WindowSeconds),
                AutoReplenishment = true,
            });
    }

    protected override string Describe(FixedWindowRateLimiterData model)
    {
        return S["{0} requests every {1} seconds, queue limit {2}", model.PermitLimit, model.WindowSeconds, model.QueueLimit];
    }
}
