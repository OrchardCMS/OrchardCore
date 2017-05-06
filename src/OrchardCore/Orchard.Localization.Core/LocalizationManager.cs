using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Orchard.Localization.Abstractions;
using System.Globalization;
using System.Threading;
using System.Linq;

namespace Orchard.Localization.Core
{
    public class LocalizationManager : ILocalizationManager
    {
        private static PluralizationRuleDelegate DefaultPluralRule = n => (n != 1 ? 1 : 0);
        private const string CacheKeyPrefix = "CultureDictionary-";

        private readonly IEnumerable<IPluralRuleProvider> _pluralRuleProviders;
        private readonly ITranslationProvider _translationProvider;
        private readonly IMemoryCache _cache;

        public LocalizationManager(
            IEnumerable<IPluralRuleProvider> pluralRuleProviders,
            ITranslationProvider translationProvider,
            IMemoryCache cache)
        {
            _pluralRuleProviders = pluralRuleProviders;
            _translationProvider = translationProvider;
            _cache = cache;
        }

        public CultureDictionary GetDictionary(CultureInfo culture)
        {
            var cachedDictionary = _cache.GetOrCreate(GetCacheKey(culture.Name), k => new Lazy<CultureDictionary>(() =>
            {
                var pluralRule = _pluralRuleProviders.OrderBy(o => o.Order).Select(o => o.GetRule(culture)).FirstOrDefault(rule => rule != null);
                var dictionary = new CultureDictionary(culture.Name, pluralRule ?? DefaultPluralRule);
                _translationProvider.LoadTranslations(culture.Name, dictionary);

                return dictionary;
            }, LazyThreadSafetyMode.ExecutionAndPublication));

            return cachedDictionary.Value;
        }

        private string GetCacheKey(string cultureName)
        {
            return CacheKeyPrefix + cultureName;
        }
    }
}
