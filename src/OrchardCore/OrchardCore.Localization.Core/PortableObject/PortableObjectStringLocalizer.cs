using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Localization.PortableObject
{
    public class PortableObjectStringLocalizer : IPluralStringLocalizer
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly ILogger _logger;
        private string _context;

        public PortableObjectStringLocalizer(string context, ILocalizationManager localizationManager, ILogger logger)
        {
            _context = context;
            _localizationManager = localizationManager;
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

            while (culture != null)
            {
                var dictionary = _localizationManager.GetDictionary(culture);

                foreach(var entry in dictionary.Translations.Select(t => new LocalizedString(t.Key, t.Value.FirstOrDefault())))
                {
                    yield return entry;
                }

                if (culture == culture.Parent)
                {
                    break;
                }

                culture = culture.Parent;
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

                if (translation == null && culture.Parent != null && culture.Parent != culture)
                {
                    dictionary = _localizationManager.GetDictionary(culture.Parent);

                    if (dictionary != null)
                    {
                        translation = dictionary[key, count]; // fallback to the parent culture
                    }
                }

                if (translation == null && context != null)
                {
                    translation = GetTranslation(name, null, culture, count); // fallback to the translation without context
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
