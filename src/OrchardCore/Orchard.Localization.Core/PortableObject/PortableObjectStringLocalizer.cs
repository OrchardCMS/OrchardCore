using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Localization.PortableObject
{
    public class PortableObjectStringLocalizer : IStringLocalizer
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly ILogger _logger;

        private CultureDictionary _dictionary;
        private CultureDictionary _parentCultureDictionary;

        public string Context { get; private set; }

        public PortableObjectStringLocalizer(CultureInfo culture, string context, ILocalizationManager localizationManager, ILogger logger)
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

                // Check if a plural form is called, which is when the only argument is of type PluralizationArgument
                if (arguments.Length == 1 && arguments[0] is PluralizationArgument pluralArgument)
                {
                    var translation = GetTranslation(name, Context, pluralArgument.Count);
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

                    var formatted = string.Format(translation ?? GetTranslation(pluralArgument.Forms, pluralArgument.Count), argumentsWithCount);
                    return new LocalizedString(name, formatted, translation == null);
                }
                else
                {
                    var translation = this[name];
                    var formatted = string.Format(translation, arguments);
                    return new LocalizedString(name, formatted, translation.ResourceNotFound);
                }
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return _dictionary.Translations.Select(t => new LocalizedString(t.Key, t.Value.FirstOrDefault()));
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return new PortableObjectStringLocalizer(culture, Context, _localizationManager, _logger);
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
