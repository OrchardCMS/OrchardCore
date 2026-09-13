using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

internal static class RateLimitLimiterValidation
{
    public static IDictionary<string, string> Validate(FixedWindowRateLimiterData data, IStringLocalizer localizer)
    {
        var errors = new Dictionary<string, string>();
        if (data.PermitLimit < 1)
        {
            errors[nameof(data.PermitLimit)] = localizer["Permit limit must be greater than zero."];
        }
        if (data.WindowSeconds < 1)
        {
            errors[nameof(data.WindowSeconds)] = localizer["Window must be greater than zero."];
        }
        if (data.QueueLimit < 0)
        {
            errors[nameof(data.QueueLimit)] = localizer["Queue limit must be nonnegative."];
        }
        return errors;
    }

    public static IDictionary<string, string> Validate(SlidingWindowRateLimiterData data, IStringLocalizer localizer)
    {
        var errors = new Dictionary<string, string>();
        if (data.PermitLimit < 1)
        {
            errors[nameof(data.PermitLimit)] = localizer["Permit limit must be greater than zero."];
        }
        if (data.WindowSeconds < 1)
        {
            errors[nameof(data.WindowSeconds)] = localizer["Window must be greater than zero."];
        }
        if (data.SegmentsPerWindow < 1)
        {
            errors[nameof(data.SegmentsPerWindow)] = localizer["Segments per window must be greater than zero."];
        }
        if (data.QueueLimit < 0)
        {
            errors[nameof(data.QueueLimit)] = localizer["Queue limit must be nonnegative."];
        }
        return errors;
    }

    public static IDictionary<string, string> Validate(ConcurrencyRateLimiterData data, IStringLocalizer localizer)
    {
        var errors = new Dictionary<string, string>();
        if (data.PermitLimit < 1)
        {
            errors[nameof(data.PermitLimit)] = localizer["Permit limit must be greater than zero."];
        }
        if (data.QueueLimit < 0)
        {
            errors[nameof(data.QueueLimit)] = localizer["Queue limit must be nonnegative."];
        }
        if (!Enum.IsDefined(data.QueueProcessingOrder))
        {
            errors[nameof(data.QueueProcessingOrder)] = localizer["Select a valid queue processing order."];
        }
        return errors;
    }

    public static IDictionary<string, string> Validate(TokenBucketRateLimiterData data, IStringLocalizer localizer)
    {
        var errors = new Dictionary<string, string>();
        if (data.TokenLimit < 1)
        {
            errors[nameof(data.TokenLimit)] = localizer["Token limit must be greater than zero."];
        }
        if (data.TokensPerPeriod < 1)
        {
            errors[nameof(data.TokensPerPeriod)] = localizer["Tokens per period must be greater than zero."];
        }
        if (data.ReplenishmentPeriodSeconds < 1)
        {
            errors[nameof(data.ReplenishmentPeriodSeconds)] = localizer["Replenishment period must be greater than zero."];
        }
        if (data.QueueLimit < 0)
        {
            errors[nameof(data.QueueLimit)] = localizer["Queue limit must be nonnegative."];
        }
        if (!Enum.IsDefined(data.QueueProcessingOrder))
        {
            errors[nameof(data.QueueProcessingOrder)] = localizer["Select a valid queue processing order."];
        }
        return errors;
    }

}
