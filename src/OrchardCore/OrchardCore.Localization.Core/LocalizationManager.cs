using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace OrchardCore.Localization
{
    public class LocalizationManager : ILocalizationManager
    {
        private static PluralizationRuleDelegate DefaultPluralRule = n => (n != 1 ? 1 : 0);
        private const string CacheKeyPrefix = "CultureDictionary-";

        private readonly IList<IPluralRuleProvider> _pluralRuleProviders;
        private readonly ITranslationProvider _translationProvider;
        private readonly IMemoryCache _cache;

        public LocalizationManager(
            IEnumerable<IPluralRuleProvider> pluralRuleProviders,
            ITranslationProvider translationProvider,
            IMemoryCache cache)
        {
            _pluralRuleProviders = pluralRuleProviders.OrderBy(o => o.Order).ToArray();
            _translationProvider = translationProvider;
            _cache = cache;
        }

        public CultureDictionary GetDictionary(CultureInfo culture)
        {
            var cachedDictionary = _cache.GetOrCreate(CacheKeyPrefix + culture.Name, k => new Lazy<CultureDictionary>(() =>
            {
                PluralizationRuleDelegate rule = DefaultPluralRule;

                foreach (var provider in _pluralRuleProviders)
                {
                    if (provider.TryGetRule(culture, out rule))
                    {
                        break;
                    }
                }

                var dictionary = new CultureDictionary(culture.Name, rule);
                _translationProvider.LoadTranslations(culture.Name, dictionary);

                return dictionary;
            }, LazyThreadSafetyMode.ExecutionAndPublication));

            return cachedDictionary.Value;
        }
    }
}
