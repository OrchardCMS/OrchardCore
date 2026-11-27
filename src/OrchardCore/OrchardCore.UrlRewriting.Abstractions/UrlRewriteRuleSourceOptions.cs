using System.Text.Json.Serialization;
using OrchardCore.Localization;

namespace OrchardCore.UrlRewriting;

/// <summary>
/// Holds the descriptions of the registered rule sources, by technical name.
/// <c>services.AddRewriteRuleSource&lt;TSource&gt;(name)</c> adds them.
/// </summary>
public class UrlRewriteRuleSourceOptions
{
    private readonly Dictionary<string, UrlRewriteRuleSourceDescriptor> _sources = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the descriptions of the registered rule sources. The key is the technical name of the rule source.
    /// </summary>
    public IReadOnlyDictionary<string, UrlRewriteRuleSourceDescriptor> Sources => _sources;

    /// <summary>
    /// Describes a rule source from the static members of its class, if no source is registered with the same name.
    /// </summary>
    /// <typeparam name="TSource">The class of the rule source.</typeparam>
    /// <param name="sourceName">The technical name of the rule source.</param>
    /// <returns>The options.</returns>
    public UrlRewriteRuleSourceOptions TryAddSource<TSource>(string sourceName)
        where TSource : class, IUrlRewriteRuleSource, IUrlRewriteRuleSourceDescription
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceName);

        _sources.TryAdd(sourceName, new UrlRewriteRuleSourceDescriptor(TSource.DisplayName, TSource.Description));

        return this;
    }
}

/// <summary>
/// Describes a registered rule source.
/// </summary>
public sealed class UrlRewriteRuleSourceDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UrlRewriteRuleSourceDescriptor"/> class.
    /// </summary>
    /// <param name="displayName">The display name of the rule source.</param>
    /// <param name="description">The description of the rule source.</param>
    public UrlRewriteRuleSourceDescriptor(LocalizationSource displayName, LocalizationSource description)
    {
        ArgumentNullException.ThrowIfNull(displayName);
        ArgumentNullException.ThrowIfNull(description);

        DisplayName = displayName;
        Description = description;
    }

    /// <summary>
    /// Gets the display name of the rule source, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    [JsonIgnore]
    public LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets the description of the rule source, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    [JsonIgnore]
    public LocalizationSource Description { get; }
}
