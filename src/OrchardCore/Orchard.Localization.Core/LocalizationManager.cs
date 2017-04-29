using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Orchard.Localization.Abstractions;
using System.Globalization;

namespace Orchard.Localization.Core
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

        public CultureDictionary GetDictionary(CultureInfo culture)
        {
            if (_cache.TryGetValue<CultureDictionary>(GetCacheKey(culture.Name), out var dictionary))
            {
                return dictionary;
            }

            dictionary = new CultureDictionary(culture.Name, _pluralRuleProvider.GetRule(culture));
            _translationProvider.LoadTranslationsToDictionary(culture.Name, dictionary);

            _cache.Set(GetCacheKey(culture.Name), dictionary);
            return dictionary;
        }

        private string GetCacheKey(string cultureName)
        {
            return string.Format(CacheKeyFormat, cultureName);
        }
    }
}
