using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Orchard.Localization
{
    public class LocalizationManager : ILocalizationManager
    {
        private readonly IPluralRuleProvider _pluralRuleProvider;
        private readonly ITranslationProvider _translationProvider;
        private readonly IMemoryCache _cache;

        private const string CacheKeyFormat = "CultureDictionary-{0}";

        public LocalizationManager(
            IPluralRuleProvider pluralRuleProvider,
            ITranslationProvider translationProvider,
            IMemoryCache cache)
        {
            _pluralRuleProvider = pluralRuleProvider;
            _translationProvider = translationProvider;
            _cache = cache;
        }

        public CultureDictionary GetDictionary(string cultureName)
        {
            if (_cache.TryGetValue<CultureDictionary>(GetCacheKey(cultureName), out var dictionary))
            {
                return dictionary;
            }

            dictionary = new CultureDictionary(cultureName, _pluralRuleProvider.GetRule(cultureName));
            _translationProvider.LoadTranslationsToDictionary(cultureName, dictionary);

            _cache.Set(GetCacheKey(cultureName), dictionary);
            return dictionary;
        }

        private string GetCacheKey(string cultureName)
        {
            return string.Format(CacheKeyFormat, cultureName);
        }
    }
}
