using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    /// <remarks>
    /// LocalizedHtmlString's arguments will be HTML encoded and not the main string. So the result
    /// should just contained the localized string containing the formatting placeholders {0...} as is.
    /// </remarks>
    public class NullHtmlLocalizerFactory : IHtmlLocalizerFactory
    {
         public IHtmlLocalizer Create(string baseName, string location) => NullHtmlLocalizer.Instance;

        public IHtmlLocalizer Create(Type resourceSource) => NullHtmlLocalizer.Instance;

        private class NullHtmlLocalizer : IHtmlLocalizer
        {
            private static readonly PluralizationRuleDelegate _defaultPluralRule = n => (n == 1) ? 0 : 1;

            public static IHtmlLocalizer Instance { get; } = new NullHtmlLocalizer();

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
                => StringLocalizer.GetAllStrings(includeParentCultures);

            public LocalizedHtmlString this[string name]
            {
                get
                {
                    var localizedString = StringLocalizer[name];

                    return new LocalizedHtmlString(localizedString.Name, localizedString.Value, localizedString.ResourceNotFound);
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

                    var localizedString = StringLocalizer[name];

                    return new LocalizedHtmlString(localizedString.Name, translation, localizedString.ResourceNotFound, arguments);
                }
            }

            public LocalizedString GetString(string name) => StringLocalizer.GetString(name);

            public LocalizedString GetString(string name, params object[] arguments)
                => StringLocalizer.GetString(name, arguments);

            IHtmlLocalizer IHtmlLocalizer.WithCulture(CultureInfo culture)
                => new NullHtmlLocalizer();

            private static IStringLocalizer StringLocalizer
                => NullStringLocalizerFactory.NullStringLocalizer.Instance;
        }
    }
}
