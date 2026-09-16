using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

internal static class RateLimitLimiterConfiguration
{
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
    };

    public static string[] Sources { get; } = ["FixedWindow", "SlidingWindow", "Concurrency", "TokenBucket"];

    public static bool IsSupported(string source) => Sources.Contains(source, StringComparer.Ordinal);

    public static JsonObject Describe(RateLimitLimiter limiter) => limiter.Source switch
    {
        "FixedWindow" => JsonSerializer.SerializeToNode(limiter.GetOrCreate<FixedWindowRateLimiterData>(), _json).AsObject(),
        "SlidingWindow" => JsonSerializer.SerializeToNode(limiter.GetOrCreate<SlidingWindowRateLimiterData>(), _json).AsObject(),
        "Concurrency" => JsonSerializer.SerializeToNode(limiter.GetOrCreate<ConcurrencyRateLimiterData>(), _json).AsObject(),
        "TokenBucket" => JsonSerializer.SerializeToNode(limiter.GetOrCreate<TokenBucketRateLimiterData>(), _json).AsObject(),
        _ => null,
    };

    public static IDictionary<string, string[]> Configure(RateLimitLimiter limiter, JsonObject values, IStringLocalizer localizer)
    {
        if (values is null)
        {
            return new Dictionary<string, string[]> { ["values"] = ["A complete limiter settings object is required."] };
        }
        try
        {
            switch (limiter.Source)
            {
                case "FixedWindow":
                    var fixedWindow = values.Deserialize<FixedWindowRateLimiterData>(_json);
                    var fixedWindowErrors = RateLimitLimiterValidation.Validate(fixedWindow, localizer);
                    if (fixedWindowErrors.Count > 0) { return Errors(fixedWindowErrors); }
                    limiter.Put(fixedWindow);
                    return new Dictionary<string, string[]>();
                case "SlidingWindow":
                    var slidingWindow = values.Deserialize<SlidingWindowRateLimiterData>(_json);
                    var slidingWindowErrors = RateLimitLimiterValidation.Validate(slidingWindow, localizer);
                    if (slidingWindowErrors.Count > 0) { return Errors(slidingWindowErrors); }
                    limiter.Put(slidingWindow);
                    return new Dictionary<string, string[]>();
                case "Concurrency":
                    var concurrency = values.Deserialize<ConcurrencyRateLimiterData>(_json);
                    var concurrencyErrors = RateLimitLimiterValidation.Validate(concurrency, localizer);
                    if (concurrencyErrors.Count > 0) { return Errors(concurrencyErrors); }
                    limiter.Put(concurrency);
                    return new Dictionary<string, string[]>();
                case "TokenBucket":
                    var tokenBucket = values.Deserialize<TokenBucketRateLimiterData>(_json);
                    var tokenBucketErrors = RateLimitLimiterValidation.Validate(tokenBucket, localizer);
                    if (tokenBucketErrors.Count > 0) { return Errors(tokenBucketErrors); }
                    limiter.Put(tokenBucket);
                    return new Dictionary<string, string[]>();
                default:
                    return new Dictionary<string, string[]> { ["source"] = ["This source has no remote configuration contract."] };
            }
        }
        catch (JsonException)
        {
            return new Dictionary<string, string[]> { ["values"] = ["Settings contain an unknown property or an invalid value type."] };
        }
    }

    private static Dictionary<string, string[]> Errors(IDictionary<string, string> errors) =>
        errors.ToDictionary(pair => "values." + JsonNamingPolicy.CamelCase.ConvertName(pair.Key), pair => new[] { pair.Value });
}
