using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization.PortableObject
{
    public class PortableObjectStringLocalizer : IPluralStringLocalizer
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly bool _fallBackToParentCulture;
        private readonly ILogger _logger;
        private string _context;

        public PortableObjectStringLocalizer(
            string context,
            ILocalizationManager localizationManager,
            bool fallBackToParentCulture,
            ILogger logger)
        {
            _context = context;
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

                var translation = GetTranslation(name, _context, CultureInfo.CurrentUICulture, null);

                return new LocalizedString(name, translation ?? name, translation == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var (translation, argumentsWithCount) = GetTranslation(name, arguments);
                var formatted = string.Format(translation.Value, argumentsWithCount);

                return new LocalizedString(name, formatted, translation.ResourceNotFound);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var culture = CultureInfo.CurrentUICulture;

            return includeParentCultures
                ? GetAllStringsFromCultureHierarchy(culture)
                : GetAllStrings(culture);
        }

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

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            // This method is never used in ASP.NET and is made obsolete in future releases.
            return this;
        }

        public (LocalizedString, object[]) GetTranslation(string name, params object[] arguments)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            // Check if a plural form is called, which is when the only argument is of type PluralizationArgument
            if (arguments.Length == 1 && arguments[0] is PluralizationArgument pluralArgument)
            {
                var translation = GetTranslation(name, _context, CultureInfo.CurrentUICulture, pluralArgument.Count);

                object[] argumentsWithCount;

                if (pluralArgument.Arguments.Length > 0)
                {
                    argumentsWithCount = new object[pluralArgument.Arguments.Length + 1];
                    argumentsWithCount[0] = pluralArgument.Count;
                    Array.Copy(pluralArgument.Arguments, 0, argumentsWithCount, 1, pluralArgument.Arguments.Length);
                }
                else
                {
                    argumentsWithCount = new object[] { pluralArgument.Count };
                }

                translation = translation ?? GetTranslation(pluralArgument.Forms, CultureInfo.CurrentUICulture, pluralArgument.Count);

                return (new LocalizedString(name, translation, translation == null), argumentsWithCount);
            }
            else
            {
                var translation = this[name];
                return (new LocalizedString(name, translation, translation.ResourceNotFound), arguments);
            }
        }

        private string GetTranslation(string[] pluralForms, CultureInfo culture, int? count)
        {
            var dictionary = _localizationManager.GetDictionary(culture);

            var pluralForm = count.HasValue ? dictionary.PluralRule(count.Value) : 0;

            if (pluralForm >= pluralForms.Length)
            {
                _logger.LogWarning($"Plural form '{pluralForm}' doesn't exist in values provided by the 'IStringLocalizer.Plural' method. Provided values: {String.Join(", ", pluralForms)}");

                // Use the latest available form
                return pluralForms[pluralForms.Length - 1];
            }

            return pluralForms[pluralForm];
        }

        private string GetTranslation(string name, string context, CultureInfo culture, int? count)
        {
            var key = CultureDictionaryRecord.GetKey(name, context);
            try
            {
                var dictionary = _localizationManager.GetDictionary(culture);

                var translation = dictionary[key, count];

                // Should we search in the parent culture?
                if (translation == null && _fallBackToParentCulture && culture.Parent != null && culture.Parent != culture)
                {
                    dictionary = _localizationManager.GetDictionary(culture.Parent);

                    if (dictionary != null)
                    {
                        translation = dictionary[key, count]; // fallback to the parent culture
                    }
                }

                // No exact translation found, search without context
                if (translation == null && context != null)
                {
                    translation = GetTranslation(name, null, culture, count);
                }

                return translation;
            }
            catch (PluralFormNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
            }

            return null;
        }
    }
}
