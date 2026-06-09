using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Core.Models;

public abstract class RateLimiterSourceBase<TModel> : IRateLimiterSource where TModel : class, new()
{
    protected RateLimiterSourceBase(IStringLocalizer stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected IStringLocalizer S { get; }

    public abstract string Name { get; }

    public abstract LocalizedString DisplayName { get; }

    public abstract LocalizedString Description { get; }

    public RateLimitPartition<string> CreatePartition(string policyName, HttpContext context, RateLimitLimiter limiter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(limiter);

        return CreatePartition(policyName, context, limiter.GetOrCreate<TModel>());
    }

    public string Describe(RateLimitLimiter limiter)
    {
        ArgumentNullException.ThrowIfNull(limiter);

        return Describe(limiter.GetOrCreate<TModel>());
    }

    protected abstract RateLimitPartition<string> CreatePartition(string policyName, HttpContext context, TModel model);

    protected abstract string Describe(TModel model);
}
