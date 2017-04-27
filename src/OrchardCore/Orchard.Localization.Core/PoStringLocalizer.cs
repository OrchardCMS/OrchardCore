using Microsoft.Extensions.Localization;
using Orchard.Localization.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Orchard.Localization.Core
{
    public class PoStringLocalizer : IStringLocalizer
    {
        private readonly ILocalizationManager _localizationManager;
        private CultureDictionary _dictionary;

        public string Context { get; private set; }

        public PoStringLocalizer(string context, ILocalizationManager localizationManager)
        {
            _localizationManager = localizationManager;
            Context = context;

            var culture = CultureInfo.CurrentUICulture.Name;
            _dictionary = localizationManager.GetDictionary(culture);
        }

        public LocalizedString this[string name]
        {
            get
            {
                var translation = GetTranslation(name);
                return new LocalizedString(name, translation ?? name, translation == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var translation = GetTranslation(name);
                var formatted = string.Format(translation ?? name, arguments);
                return new LocalizedString(name, formatted, translation == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            //Is not neccessary
            return null;
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            _dictionary = _localizationManager.GetDictionary(culture.TwoLetterISOLanguageName);
            return this;
        }

        private string GetTranslation(string name, params object[] arguments)
        {
            var key = CultureDictionaryRecord.GetKey(name, Context);
            var translation = _dictionary[key]?[0];

            return translation;
        }
    }
}
