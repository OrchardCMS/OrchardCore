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

                string translation;
                var defaultTranslation = name;

                var pluralArgument = arguments.FirstOrDefault() as PluralArgument;
                if(pluralArgument != null)
                {
                    defaultTranslation = pluralArgument.Count != 1 ? pluralArgument.PluralText : name;
                    arguments = arguments.Length > 1 ? (object[])arguments[1] : new object[0];
                }

                translation = GetTranslation(name, Context, pluralArgument?.Count);
                var formatted = string.Format(translation ?? defaultTranslation, arguments);
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
            } catch (PluralFormNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
            }

            return null;
        }
    }
}
