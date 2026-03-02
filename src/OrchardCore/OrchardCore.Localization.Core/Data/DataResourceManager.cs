using System.Globalization;
using Microsoft.Extensions.Caching.Memory;

namespace OrchardCore.Localization.Data;

public class DataResourceManager
{
    private const string CacheKeyPrefix = "DataCultureDictionary-";

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
        var dictionary = GetCultureDictionaryAsync(culture)
            .AsTask()
            .GetAwaiter()
            .GetResult();


        if (dictionary != null)
        {
            var key = CultureDictionaryRecord.GetKey(name, context);

            value = dictionary[key];
        }

        return value;
    }

    public async ValueTask<IDictionary<string, string>> GetResourcesAsync(CultureInfo culture, bool tryParents)
    {
        ArgumentNullException.ThrowIfNull(culture, nameof(culture));

        var currentCulture = culture;

        var cultureDictionary = await GetCultureDictionaryAsync(culture);

        var resources = cultureDictionary.Translations
            .ToDictionary(t => t.Key.ToString(), t => t.Value[0]);

        if (tryParents)
        {
            do
            {
                currentCulture = currentCulture.Parent;

                var currentCultureDictionary = await GetCultureDictionaryAsync(currentCulture);

                var currentResources = currentCultureDictionary.Translations
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

    private async ValueTask<CultureDictionary> GetCultureDictionaryAsync(CultureInfo culture)
    {
        var cachedDictionary = await _cache.GetOrCreateAsync(CacheKeyPrefix + culture.Name, async k =>
        {
            var dictionary = new CultureDictionary(culture.Name, _noPluralRule);

            await _translationProvider.LoadTranslationsAsync(culture.Name, dictionary);

            return dictionary;
        });

        return cachedDictionary;
    }
}
