using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Core;

/// <summary>
/// Defines a reusable source that can create and describe a concrete rate limiter configuration.
/// </summary>
public interface IRateLimiterSource
{
    /// <summary>
    /// Gets the stable identifier used to persist this limiter source.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the localized display name shown in the admin UI.
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// Gets the localized description shown in the admin UI.
    /// </summary>
    LocalizedString Description { get; }

    /// <summary>
    /// Creates a rate-limiter partition for the specified policy and request context.
    /// </summary>
    /// <param name="policyName">The name of the policy that owns the limiter.</param>
    /// <param name="context">The current HTTP request context.</param>
    /// <param name="limiter">The stored limiter definition.</param>
    /// <returns>The rate-limiter partition to apply to the request.</returns>
    RateLimitPartition<string> CreatePartition(string policyName, HttpContext context, RateLimitLimiter limiter);

    /// <summary>
    /// Produces a human-readable summary for the specified limiter definition.
    /// </summary>
    /// <param name="limiter">The stored limiter definition.</param>
    /// <returns>A localized summary of the limiter settings.</returns>
    string Describe(RateLimitLimiter limiter);
}
