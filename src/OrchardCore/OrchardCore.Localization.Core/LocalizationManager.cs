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
        private readonly IEnumerable<ITranslationProvider> _translationProviders;
        private readonly IMemoryCache _cache;

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
        {
            _pluralRuleProviders = pluralRuleProviders.OrderBy(o => o.Order).ToArray();
            _translationProviders = translationProviders;
            _cache = cache;
        }

        /// <inheritdoc />
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
                foreach (var translationProvider in _translationProviders)
                {
                    translationProvider.LoadTranslations(culture.Name, dictionary);
                }

                return dictionary;
            }, LazyThreadSafetyMode.ExecutionAndPublication));

            return cachedDictionary.Value;
        }
    }
}
