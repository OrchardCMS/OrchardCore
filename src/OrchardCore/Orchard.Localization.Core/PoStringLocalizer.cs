using Microsoft.Extensions.Localization;
using Orchard.Localization.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Orchard.Localization.Core
{
    public class PoStringLocalizer : IStringLocalizer
    {
        private readonly ILocalizationManager _localizationManager;
        private CultureDictionary _dictionary;
        private CultureDictionary _parentCultureDictionary;

        public string Context { get; private set; }

        public PoStringLocalizer(CultureInfo culture, string context, ILocalizationManager localizationManager)
        {
            Context = context;
            _localizationManager = localizationManager;
            _dictionary = localizationManager.GetDictionary(culture);

            if(culture.Parent != null)
            {
                _parentCultureDictionary = localizationManager.GetDictionary(culture.Parent);
            }
        }

        public LocalizedString this[string name]
        {
            get
            {
                var translation = GetTranslation(name, Context);
                return new LocalizedString(name, translation ?? name, translation == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var translation = GetTranslation(name, Context);
                var formatted = string.Format(translation ?? name, arguments);
                return new LocalizedString(name, formatted, translation == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return _dictionary.Translations.Select(t => new LocalizedString(t.Key, t.Value.FirstOrDefault()));
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return new PoStringLocalizer(culture, Context, _localizationManager);
        }

        private string GetTranslation(string name, string context)
        {
            var key = CultureDictionaryRecord.GetKey(name, context);
            var translation = _dictionary[key]?[0];
            translation = translation ?? _parentCultureDictionary?[key]?[0]; // falback to the parent culture
            if (context != null)
            {
                translation = translation ?? GetTranslation(name, null); // falback to the translation without context
            }

            return translation;
        }
    }
}
