using System.Globalization;
using Microsoft.Extensions.Caching.Memory;

namespace OrchardCore.Localization;

/// <summary>
/// Represents a manager that manage the localization resources.
/// </summary>
public class LocalizationManager : ILocalizationManager
{
    private const string CacheKeyPrefix = "CultureDictionary-";

    private static readonly PluralizationRuleDelegate s_defaultPluralRule = n => (n != 1 ? 1 : 0);

    private readonly IList<IPluralRuleProvider> _pluralRuleProviders;
    private readonly IEnumerable<ITranslationProvider> _translationProviders;
    private readonly IMemoryCache _cache;
    private readonly IEnumerable<ITranslationOverrideProvider> _translationOverrides;

    /// <summary>
    /// Creates a new instance of <see cref="LocalizationManager"/>.
    /// </summary>
    /// <param name="pluralRuleProviders">A list of <see cref="IPluralRuleProvider"/>s.</param>
    /// <param name="translationProviders">The list of available <see cref="ITranslationProvider"/>.</param>
    /// <param name="cache">The <see cref="IMemoryCache"/>.</param>
    public LocalizationManager(
        IEnumerable<IPluralRuleProvider> pluralRuleProviders,
        IEnumerable<ITranslationProvider> translationProviders,
        IMemoryCache cache)
        : this(pluralRuleProviders, translationProviders, cache, [])
    {
    }

    /// <summary>
    /// Creates a localization manager with versioned translation overrides.
    /// </summary>
    /// <param name="pluralRuleProviders">The available plural rules.</param>
    /// <param name="translationProviders">The standard translation sources.</param>
    /// <param name="cache">The culture dictionary cache.</param>
    /// <param name="translationOverrides">The versioned translation overlays.</param>
    public LocalizationManager(
        IEnumerable<IPluralRuleProvider> pluralRuleProviders,
        IEnumerable<ITranslationProvider> translationProviders,
        IMemoryCache cache,
        IEnumerable<ITranslationOverrideProvider> translationOverrides)
    {
        _pluralRuleProviders = pluralRuleProviders.OrderBy(o => o.Order).ToArray();
        _translationProviders = translationProviders;
        _cache = cache;
        _translationOverrides = translationOverrides ?? [];
    }

    /// <inheritdoc />
    public CultureDictionary GetDictionary(CultureInfo culture)
    {
        var cachedDictionary = _cache.GetOrCreate(CacheKeyPrefix + culture.Name, k => new Lazy<CultureDictionary>(() =>
        {
            var rule = s_defaultPluralRule;

            foreach (var provider in _pluralRuleProviders)
            {
                if (provider.TryGetRule(culture, out rule))
                {
                    break;
                }
            }

            var dictionary = new CultureDictionary(culture.Name, rule ?? s_defaultPluralRule);
            foreach (var translationProvider in _translationProviders)
            {
                translationProvider.LoadTranslations(culture.Name, dictionary);
            }

            return dictionary;
        }, LazyThreadSafetyMode.ExecutionAndPublication));

        var dictionary = cachedDictionary.Value;
        foreach (var provider in _translationOverrides)
        {
            dictionary = provider.ApplyTranslations(dictionary);
        }

        return dictionary;
    }
}
