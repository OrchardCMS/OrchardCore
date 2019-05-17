using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization
{
    public class YesSqlDataLocalizerFactory : IDataLocalizerFactory
    {
        private readonly ConcurrentDictionary<string, YesSqlDataLocalizer> _localizerCache = new ConcurrentDictionary<string, YesSqlDataLocalizer>();
        private readonly bool _fallBackToParentUICultures;
        private readonly ILoggerFactory _loggerFactory;

        public YesSqlDataLocalizerFactory(
            ILoggerFactory loggerFactory,
            IOptions<RequestLocalizationOptions> requestLocalizationOptions)
        {
            _fallBackToParentUICultures = requestLocalizationOptions.Value.FallBackToParentUICultures;
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IDataLocalizer Create()
        {
            var culture = CultureInfo.CurrentUICulture;

            return _localizerCache.GetOrAdd($"C={culture.Name}", _ =>
            {
                //TODO: Fetch the data from the database
                CultureDictionary dictionary = null;

                if (_fallBackToParentUICultures)
                {
                    dictionary = new CultureDictionary(culture.Name, null);
                    dictionary.MergeTranslations(GetResourcesFromCultureHierarchy(culture));
                }
                else
                {
                    dictionary = GetDictionaries().SingleOrDefault(d => d.CultureName == culture.Name);
                }

                return new YesSqlDataLocalizer(dictionary, _loggerFactory.CreateLogger<DataLocalizer>());
            });
        }

        private IEnumerable<CultureDictionaryRecord> GetResourcesFromCultureHierarchy(CultureInfo culture)
        {
            var currentCulture = culture;
            var records = new List<CultureDictionaryRecord>();

            while (true)
            {
                var cultureResources = GetDictionaries().Where(c => c.CultureName == currentCulture.Name)
                    .SelectMany(c => c.Translations);

                if (cultureResources != null)
                {
                    foreach (var resource in cultureResources)
                    {
                        yield return new CultureDictionaryRecord(resource.Key, null, resource.Value);
                    }
                }

                if (currentCulture == currentCulture.Parent)
                {
                    break;
                }

                currentCulture = currentCulture.Parent;
            }
        }

        private IEnumerable<CultureDictionary> GetDictionaries()
        {
            yield return SetupDictionary("ar-YE", new List<CultureDictionaryRecord>
                {
                    new CultureDictionaryRecord("Bye", null, new[] { "مع السلامة" })
                });
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