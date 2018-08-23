using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    public class PluralizeStringLocalizer<TResourceSource> : IPluralizeStringLocalizer<TResourceSource>
    {
        private IPluralizeStringLocalizer _localizer;

        public PluralizeStringLocalizer(IStringLocalizerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _localizer = (IPluralizeStringLocalizer)factory.Create(typeof(TResourceSource));
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                return _localizer[name];
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

                return _localizer[name, arguments];
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            _localizer.GetAllStrings(includeParentCultures);

        public PluralizationRule GetPluralRule(string twoLetterISOLanguageName) =>
            _localizer.GetPluralRule(twoLetterISOLanguageName);

        public LocalizedString Pluralize(string name, int count) =>
            _localizer.Pluralize(name, count);

        public IStringLocalizer WithCulture(CultureInfo culture) =>
            _localizer.WithCulture(culture);
    }
}
