using OrchardCore.Localization;

namespace OrchardCore.RateLimits.Core;

/// <summary>
/// Describes an <see cref="IRateLimiterSource"/> with the static members of its class, so the limiter sources can be
/// listed without creating them, and are translated for the current culture when they are shown. Every source
/// registered with <c>services.AddRateLimiterSource&lt;TSource&gt;(name)</c> implements it. It is separate from
/// <see cref="IRateLimiterSource"/>, because an interface with static abstract members can't be used as a type argument.
/// </summary>
public interface IRateLimiterSourceDescription
{
    /// <summary>
    /// Gets the display name shown in the admin UI, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    static abstract LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets the description shown in the admin UI, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    static abstract LocalizationSource Description { get; }
}
