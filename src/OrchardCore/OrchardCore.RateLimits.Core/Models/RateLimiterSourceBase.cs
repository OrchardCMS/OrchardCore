using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Core.Models;

/// <summary>
/// Provides a typed base implementation for limiter sources backed by a specific settings model.
/// </summary>
public abstract class RateLimiterSourceBase<TModel> : IRateLimiterSource where TModel : class, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterSourceBase{TModel}"/> class.
    /// </summary>
    /// <param name="stringLocalizer">The string localizer used by the limiter source.</param>
    protected RateLimiterSourceBase(IStringLocalizer stringLocalizer)
    {
        S = stringLocalizer;
    }

    /// <summary>
    /// Gets the localizer used for display names and descriptions.
    /// </summary>
    protected IStringLocalizer S { get; }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract LocalizedString DisplayName { get; }

    /// <inheritdoc />
    public abstract LocalizedString Description { get; }

    /// <inheritdoc />
    public RateLimitPartition<string> CreatePartition(string policyName, HttpContext context, RateLimitLimiter limiter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(limiter);

        return CreatePartition(policyName, context, limiter.GetOrCreate<TModel>());
    }

    /// <inheritdoc />
    public string Describe(RateLimitLimiter limiter)
    {
        ArgumentNullException.ThrowIfNull(limiter);

        return Describe(limiter.GetOrCreate<TModel>());
    }

    /// <summary>
    /// Creates a typed rate-limiter partition from the resolved settings model.
    /// </summary>
    /// <param name="policyName">The name of the policy that owns the limiter.</param>
    /// <param name="context">The current HTTP request context.</param>
    /// <param name="model">The typed limiter settings.</param>
    /// <returns>The rate-limiter partition to apply.</returns>
    protected abstract RateLimitPartition<string> CreatePartition(string policyName, HttpContext context, TModel model);

    /// <summary>
    /// Produces a human-readable description of the typed limiter settings.
    /// </summary>
    /// <param name="model">The typed limiter settings.</param>
    /// <returns>A localized summary of the limiter settings.</returns>
    protected abstract string Describe(TModel model);
}
