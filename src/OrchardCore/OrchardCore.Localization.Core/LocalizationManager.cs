using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a manager that manage the localization resources.
    /// </summary>
    public class LocalizationManager : ILocalizationManager
    {
        private const string CacheKeyPrefix = "CultureDictionary-";

        private static PluralizationRuleDelegate DefaultPluralRule = n => (n != 1 ? 1 : 0);

        private readonly IList<IPluralRuleProvider> _pluralRuleProviders;
        private readonly ITranslationProvider _translationProvider;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Creates a new instance of <see cref="LocalizationManager"/>.
        /// </summary>
        /// <param name="pluralRuleProviders">A list of <see cref="IPluralRuleProvider"/>s.</param>
        /// <param name="translationProvider">The <see cref="ITranslationProvider"/>.</param>
        /// <param name="cache">The <see cref="IMemoryCache"/>.</param>
        public LocalizationManager(
            IEnumerable<IPluralRuleProvider> pluralRuleProviders,
            ITranslationProvider translationProvider,
            IMemoryCache cache)
        {
            _pluralRuleProviders = pluralRuleProviders.OrderBy(o => o.Order).ToArray();
            _translationProvider = translationProvider;
            _cache = cache;
        }

        /// <inheritdocs />
        public CultureDictionary GetDictionary(CultureInfo culture)
        {
            var cachedDictionary = _cache.GetOrCreate(CacheKeyPrefix + culture.Name, k => new Lazy<CultureDictionary>(() =>
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
}
