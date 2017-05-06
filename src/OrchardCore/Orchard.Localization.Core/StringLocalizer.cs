using Microsoft.Extensions.Localization;
using Orchard.Localization.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Orchard.Localization.Core
{
    public class StringLocalizer : IStringLocalizer
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly ILogger _logger;

        private CultureDictionary _dictionary;
        private CultureDictionary _parentCultureDictionary;

        public string Context { get; private set; }

        public StringLocalizer(CultureInfo culture, string context, ILocalizationManager localizationManager, ILogger logger)
        {
            Context = context;
            _localizationManager = localizationManager;
            _logger = logger;
            _dictionary = localizationManager.GetDictionary(culture);

            if (culture.Parent != null)
            {
                _parentCultureDictionary = localizationManager.GetDictionary(culture.Parent);
            }
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var translation = GetTranslation(name, Context, null);
                return new LocalizedString(name, translation ?? name, translation == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var defaultPluralForms = new[] { name };
                int? count = null;

                if (arguments.Length >= 2 && arguments[arguments.Length - 2] is int argumentCount && arguments[arguments.Length - 1] is string[] pluralForms)
                {
                    count = argumentCount;
                    defaultPluralForms = pluralForms;
                    Array.Resize(ref arguments, arguments.Length - 2); // remove plural related data from arguments
                }

                var translation = GetTranslation(name, Context, count);
                var formatted = string.Format(translation ?? GetTranslation(defaultPluralForms, count), arguments);
                return new LocalizedString(name, formatted, translation == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return _dictionary.Translations.Select(t => new LocalizedString(t.Key, t.Value.FirstOrDefault()));
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return new StringLocalizer(culture, Context, _localizationManager, _logger);
        }

        private string GetTranslation(string[] pluralForms, int? count)
        {
            var pluralForm = count.HasValue ? _dictionary.PluralRule(count.Value) : 0;
            if (pluralForm >= pluralForms.Length)
            {
                throw new PluralFormNotFoundException($"Plural form '{pluralForm}' doesn't exist in values provided by the IStringLocalizer.Plural method. Provided values: {pluralForms}");
            }

            return pluralForms[pluralForm];
        }

        private string GetTranslation(string name, string context, int? count)
        {
            var key = CultureDictionaryRecord.GetKey(name, context);
            try
            {
                var translation = _dictionary[key, count];

                if (translation == null && _parentCultureDictionary != null)
                {
                    translation = _parentCultureDictionary[key, count]; // fallback to the parent culture
                }

                if (translation == null && context != null)
                {
                    translation = GetTranslation(name, null, count); // fallback to the translation without context
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
