using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Localization
{
    public class YesSqlDataLocalizerFactory : IDataLocalizerFactory
    {
        private readonly ConcurrentDictionary<string, YesSqlDataLocalizer> _localizerCache = new ConcurrentDictionary<string, YesSqlDataLocalizer>();
        private readonly ILoggerFactory _loggerFactory;

        public YesSqlDataLocalizerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IDataLocalizer Create()
        {
            var culture = CultureInfo.CurrentUICulture.Name;

            return _localizerCache.GetOrAdd($"C={culture}", _ =>
            {
                //TODO: Fetch the data from the database
                var dictionary = GetDictionaries().SingleOrDefault(d => d.CultureName == culture);

                return new YesSqlDataLocalizer(dictionary, _loggerFactory.CreateLogger<DataLocalizer>());
            });
        }

        private IEnumerable<CultureDictionary> GetDictionaries()
        {
            yield return SetupDictionary("ar", new List<CultureDictionaryRecord>
                {
                    new CultureDictionaryRecord("Hello", null, new[] { "مرحبا" }),
                    new CultureDictionaryRecord("Bye", null, new[] { "مع السلامة" })
                });
            yield return SetupDictionary("fr-FR", new List<CultureDictionaryRecord>
                {
                    new CultureDictionaryRecord("Hello", null, new[] { "Bonjour" }),
                    new CultureDictionaryRecord("Bye", null, new[] { "au revoir" })
                });
        }

        private CultureDictionary SetupDictionary(string culture, IEnumerable<CultureDictionaryRecord> records)
        {
            var dictionary = new CultureDictionary(culture, null);
            dictionary.MergeTranslations(records);

            return dictionary;
        }
    }
}