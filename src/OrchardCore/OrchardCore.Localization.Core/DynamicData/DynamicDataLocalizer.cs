using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Localization.DynamicData
{
    public class DynamicDataLocalizer : IDataLocalizer
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly bool _fallBackToParentCulture;
        private readonly ILogger _logger;

        public DynamicDataLocalizer(
            ILocalizationManager localizationManager,
            bool fallBackToParentCulture,
            ILogger logger)
        {
            _localizationManager = localizationManager;
            _fallBackToParentCulture = fallBackToParentCulture;
            _logger = logger;
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var translation = GetTranslation(name, CultureInfo.CurrentUICulture);

                return new LocalizedString(name, translation ?? name, translation == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var translation = GetTranslation(name, CultureInfo.CurrentUICulture);
                var formatted = string.Format(translation, arguments);

                return new LocalizedString(name, formatted, translation == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var culture = CultureInfo.CurrentUICulture;

            return includeParentCultures
                ? GetAllStringsFromCultureHierarchy(culture)
                : GetAllStrings(culture);
        }

        public IStringLocalizer WithCulture(CultureInfo culture) => this;

        private IEnumerable<LocalizedString> GetAllStringsFromCultureHierarchy(CultureInfo culture)
        {
            var currentCulture = culture;
            var allLocalizedStrings = new List<LocalizedString>();

            do
            {
                var localizedStrings = GetAllStrings(currentCulture);

                if (localizedStrings != null)
                {
                    foreach (var localizedString in localizedStrings)
                    {
                        if (!allLocalizedStrings.Any(ls => ls.Name == localizedString.Name))
                        {
                            allLocalizedStrings.Add(localizedString);
                        }
                    }
                }

                currentCulture = currentCulture.Parent;
            } while (currentCulture != currentCulture.Parent);

            return allLocalizedStrings;
        }

        private IEnumerable<LocalizedString> GetAllStrings(CultureInfo culture)
        {
            var dictionary = _localizationManager.GetDictionary(culture);

            foreach (var translation in dictionary.Translations)
            {
                yield return new LocalizedString(translation.Key, translation.Value.FirstOrDefault());
            }
        }

        private string GetTranslation(string name, CultureInfo culture)
        {
            var count = 0;
            var dictionary = _localizationManager.GetDictionary(culture);
            var translation = dictionary[new CultureDictionaryRecordKey(name), count];

            if (translation == null && _fallBackToParentCulture && culture.Parent != null && culture.Parent != culture)
            {
                dictionary = _localizationManager.GetDictionary(culture.Parent);

                if (dictionary != null)
                {
                    translation = dictionary[new CultureDictionaryRecordKey(name), count];
                }
            }

            return translation;
        }
    }
}
