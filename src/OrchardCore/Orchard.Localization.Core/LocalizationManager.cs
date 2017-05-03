using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Orchard.Localization.Abstractions;
using System.Globalization;
using System.Threading;

namespace Orchard.Localization.Core
{
    public class LocalizationManager : ILocalizationManager
    {
        private static Func<int, int> DefaultRule = n => (n != 1 ? 1 : 0);
        private const string CacheKeyPrefix = "CultureDictionary-";

        private readonly IPluralRuleProvider _pluralRuleProvider;
        private readonly ITranslationProvider _translationProvider;
        private readonly IMemoryCache _cache;
        
        public LocalizationManager(
            IPluralRuleProvider pluralRuleProvider,
            ITranslationProvider translationProvider,
            IMemoryCache cache)
        {
            _pluralRuleProvider = pluralRuleProvider;
            _translationProvider = translationProvider;
            _cache = cache;
        }

        public CultureDictionary GetDictionary(CultureInfo culture)
        {
            var cachedDictionary = _cache.GetOrCreate(GetCacheKey(culture.Name), k => new Lazy<CultureDictionary>(() =>
            {
                var dictionary = new CultureDictionary(culture.Name, _pluralRuleProvider.GetRule(culture));
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
