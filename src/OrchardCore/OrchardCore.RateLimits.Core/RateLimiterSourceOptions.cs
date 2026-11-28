using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Localization;

namespace OrchardCore.RateLimits.Core;

/// <summary>
/// Holds the descriptions of the registered limiter sources, by technical name.
/// <c>services.AddRateLimiterSource&lt;TSource&gt;(name)</c> adds them.
/// </summary>
public class RateLimiterSourceOptions
{
    private readonly Dictionary<string, RateLimiterSourceDescriptor> _sources = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the descriptions of the registered limiter sources. The key is the technical name of the source.
    /// </summary>
    public IReadOnlyDictionary<string, RateLimiterSourceDescriptor> Sources => _sources;

    /// <summary>
    /// Describes a limiter source from the static members of its class, if no source is registered with the same name.
    /// </summary>
    /// <typeparam name="TSource">The class of the limiter source.</typeparam>
    /// <param name="sourceName">The technical name of the limiter source.</param>
    /// <returns>The options.</returns>
    public RateLimiterSourceOptions TryAddSource<TSource>(string sourceName)
        where TSource : class, IRateLimiterSource, IRateLimiterSourceDescription
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceName);

        _sources.TryAdd(sourceName, new RateLimiterSourceDescriptor(TSource.DisplayName, TSource.Description));

        return this;
    }
}

/// <summary>
/// Describes a registered limiter source.
/// </summary>
public sealed class RateLimiterSourceDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterSourceDescriptor"/> class.
    /// </summary>
    /// <param name="displayName">The display name of the limiter source.</param>
    /// <param name="description">The description of the limiter source.</param>
    public RateLimiterSourceDescriptor(LocalizationSource displayName, LocalizationSource description)
    {
        ArgumentNullException.ThrowIfNull(displayName);
        ArgumentNullException.ThrowIfNull(description);

        DisplayName = displayName;
        Description = description;
    }

    /// <summary>
    /// Gets the display name of the limiter source, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    [JsonIgnore]
    public LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets the description of the limiter source, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    [JsonIgnore]
    public LocalizationSource Description { get; }
}

/// <summary>
/// Registers limiter sources.
/// </summary>
public static class RateLimiterSourceServiceCollectionExtensions
{
    /// <summary>
    /// Registers a limiter source as a singleton keyed by its technical name, and describes it in
    /// <see cref="RateLimiterSourceOptions"/> from its static members.
    /// </summary>
    /// <typeparam name="TSource">The class of the limiter source.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="sourceName">The technical name of the limiter source.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRateLimiterSource<TSource>(this IServiceCollection services, string sourceName)
        where TSource : class, IRateLimiterSource, IRateLimiterSourceDescription
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceName);

        services.Configure<RateLimiterSourceOptions>(options => options.TryAddSource<TSource>(sourceName));

        return services.AddKeyedSingleton<IRateLimiterSource, TSource>(sourceName);
    }
}
