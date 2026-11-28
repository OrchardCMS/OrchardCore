using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Core.Models;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

/// <summary>
/// Creates fixed-window rate-limit partitions for policy limiters.
/// </summary>
public sealed class FixedWindowRateLimiterSource : RateLimiterSourceBase<FixedWindowRateLimiterData>, IRateLimiterSourceDescription
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

    /// <inheritdoc cref="IRateLimiterSourceDescription.DisplayName" />
    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<FixedWindowRateLimiterSource>("Fixed window");

    /// <inheritdoc cref="IRateLimiterSourceDescription.Description" />
    public static LocalizationSource Description { get; } = LocalizationSource.Create<FixedWindowRateLimiterSource>("Allows a set number of requests during each time period, then resets the count all at once.");

    protected override RateLimitPartition<string> CreatePartition(string policyName, HttpContext context, FixedWindowRateLimiterData model)
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{policyName}:{SourceName}:{RateLimitPartitionHelpers.GetClientPartitionKey(context)}",
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
