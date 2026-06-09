using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Core.Models;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

public sealed class TokenBucketRateLimiterSource : RateLimiterSourceBase<TokenBucketRateLimiterData>
{
    public const string SourceName = "TokenBucket";

    public TokenBucketRateLimiterSource(IStringLocalizer<TokenBucketRateLimiterSource> stringLocalizer)
        : base(stringLocalizer)
    {
    }

    public override string Name => SourceName;

    public override LocalizedString DisplayName => S["Token bucket"];

    public override LocalizedString Description => S["Accumulates tokens over time and spends them per request per IP."];

    protected override RateLimitPartition<string> CreatePartition(string policyName, HttpContext context, TokenBucketRateLimiterData model)
    {
        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: $"{policyName}:{SourceName}:{RateLimitPartitionHelpers.GetRemoteIpAddress(context)}",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = model.TokenLimit,
                QueueLimit = model.QueueLimit,
                TokensPerPeriod = model.TokensPerPeriod,
                ReplenishmentPeriod = TimeSpan.FromSeconds(model.ReplenishmentPeriodSeconds),
                QueueProcessingOrder = model.QueueProcessingOrder,
                AutoReplenishment = true,
            });
    }

    protected override string Describe(TokenBucketRateLimiterData model)
    {
        return S["{0} tokens, {1} replenished every {2} seconds, queue limit {3}, queue order {4}", model.TokenLimit, model.TokensPerPeriod, model.ReplenishmentPeriodSeconds, model.QueueLimit, model.QueueProcessingOrder];
    }
}
