using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    public class NullHtmlLocalizer : IHtmlLocalizer
    {
        private static readonly PluralizationRuleDelegate _defaultPluralRule = n => (n == 1) ? 0 : 1;

        public static NullHtmlLocalizer Instance { get; } = new NullHtmlLocalizer();

        public LocalizedHtmlString this[string name]
        {
            get
            {
                var localizedString = NullStringLocalizer.Instance[name];

                return new LocalizedHtmlString(localizedString.Name, localizedString.Value, true);
            }
        }

        public LocalizedHtmlString this[string name, params object[] arguments]
        {
            get
            {
                var translation = name;

                if (arguments.Length == 1 && arguments[0] is PluralizationArgument pluralArgument)
                {
                    translation = pluralArgument.Forms[_defaultPluralRule(pluralArgument.Count)];

                    arguments = new object[pluralArgument.Arguments.Length + 1];
                    arguments[0] = pluralArgument.Count;
                    Array.Copy(pluralArgument.Arguments, 0, arguments, 1, pluralArgument.Arguments.Length);
                }

                return new LocalizedHtmlString(name, translation, false, arguments);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => NullStringLocalizer.Instance.GetAllStrings(includeParentCultures);

        public LocalizedString GetString(string name)
            => NullStringLocalizer.Instance.GetString(name);

        public LocalizedString GetString(string name, params object[] arguments)
            => NullStringLocalizer.Instance.GetString(name, arguments);

        [Obsolete("This method will be removed in the upcoming ASP.NET Core major release.")]
        public IStringLocalizer WithCulture(CultureInfo culture)
            => NullStringLocalizer.Instance.WithCulture(culture);
    }
}
