using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace OrchardCore.Localization.Data;

public class DataResourceManager
{
    private const string _cacheKeyPrefix = "CultureDictionary-";

    private static readonly PluralizationRuleDelegate DefaultPluralRule = n => (n != 1 ? 1 : 0);

    private readonly IDataTranslationProvider _translationProvider;
    private readonly IList<IPluralRuleProvider> _pluralRuleProviders;
    private readonly IMemoryCache _cache;

    public DataResourceManager(
        IDataTranslationProvider translationProvider,
        IEnumerable<IPluralRuleProvider> pluralRuleProviders,
        IMemoryCache cache)
    {
        _translationProvider = translationProvider;
        _pluralRuleProviders = pluralRuleProviders.OrderBy(o => o.Order).ToArray();
        _cache = cache;
    }

    public string GetString(string name, string context) => GetString(name, context, null);

    public string GetString(string name, string context, CultureInfo culture)
    {
        if (String.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
        }

        if (String.IsNullOrEmpty(context))
        {
            throw new ArgumentException($"'{nameof(context)}' cannot be null or empty.", nameof(context));
        }

        culture ??= CultureInfo.CurrentUICulture;

        string value = null;
        var dictionary = GetCultureDictionary(culture);


        if (dictionary != null)
        {
            var key = CultureDictionaryRecord.GetKey(name, context);

            value = dictionary[key];
        }

        return value;
    }

    public IDictionary<string, string> GetResources(CultureInfo culture, bool tryParents)
    {
        if (culture is null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        var currentCulture = culture;

        var resources = GetCultureDictionary(culture).Translations
            .ToDictionary(t => t.Key.ToString(), t => t.Value[0]);

        if (tryParents)
        {
            do
            {
                currentCulture = currentCulture.Parent;

                var currentResources = GetCultureDictionary(currentCulture).Translations
                    .ToDictionary(t => t.Key.ToString(), t => t.Value[0]);

                foreach (var translation in currentResources)
                {
                    if (!resources.Any(r => r.Key == translation.Key))
                    {
                        resources.Add(translation.Key, translation.Value);
                    }
                }
            } while (currentCulture != CultureInfo.InvariantCulture);
        }

        return resources;
    }

    private CultureDictionary GetCultureDictionary(CultureInfo culture)
    {
        var cachedDictionary = _cache.GetOrCreate(_cacheKeyPrefix + culture.Name, k => new Lazy<CultureDictionary>(() =>
        {
            var rule = DefaultPluralRule;

            foreach (var provider in _pluralRuleProviders)
            {
                if (provider.TryGetRule(culture, out rule))
                {
                    break;
                }
            }

            var dictionary = new CultureDictionary(culture.Name, rule ?? DefaultPluralRule);
            _translationProvider.LoadTranslations(culture.Name, dictionary);

            return dictionary;
        }, LazyThreadSafetyMode.ExecutionAndPublication));

        return cachedDictionary.Value;
    }
}
