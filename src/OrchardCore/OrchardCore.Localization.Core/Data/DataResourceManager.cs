using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace OrchardCore.Localization.Data;

public class DataResourceManager
{
    private const string CacheKeyPrefix = "CultureDictionary-";

    private static readonly PluralizationRuleDelegate _noPluralRule = n => 0;

    private readonly IDataTranslationProvider _translationProvider;
    private readonly IMemoryCache _cache;

    public DataResourceManager(IDataTranslationProvider translationProvider, IMemoryCache cache)
    {
        _translationProvider = translationProvider;
        _cache = cache;
    }

    public string GetString(string name, string context) => GetString(name, context, null);

    public string GetString(string name, string context, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentNullException.ThrowIfNullOrEmpty(context, nameof(context));

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
        ArgumentNullException.ThrowIfNull(culture, nameof(culture));

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
        var cachedDictionary = _cache.GetOrCreate(CacheKeyPrefix + culture.Name, k => new Lazy<CultureDictionary>(() =>
        {
            var dictionary = new CultureDictionary(culture.Name, _noPluralRule);

            _translationProvider.LoadTranslations(culture.Name, dictionary);

            return dictionary;
        }, LazyThreadSafetyMode.ExecutionAndPublication));

        return cachedDictionary.Value;
    }
}
