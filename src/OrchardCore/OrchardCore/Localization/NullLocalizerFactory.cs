using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    /// <remarks>
    /// A LocalizedString is not encoded, so it can contain the formatted string
    /// including the argument values.
    /// However a LocalizedHtmlString's arguments will be HTML encoded and not the main string. So the result
    /// should just contained the localized string containing the formatting placeholders {0...} as is.
    /// </remarks>
    public class NullLocalizerFactory : IStringLocalizerFactory, IHtmlLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource) => NullLocalizer.Instance;

        public IStringLocalizer Create(string baseName, string location) => NullLocalizer.Instance;

        IHtmlLocalizer IHtmlLocalizerFactory.Create(string baseName, string location) => NullLocalizer.Instance;

        IHtmlLocalizer IHtmlLocalizerFactory.Create(Type resourceSource) => NullLocalizer.Instance;

        private class NullLocalizer : IStringLocalizer, IHtmlLocalizer
        {
            private static readonly PluralizationRuleDelegate _defaultPluralRule = n => (n == 1) ? 0 : 1;

            public static NullLocalizer Instance { get; } = new NullLocalizer();

            public LocalizedString this[string name] => new LocalizedString(name, name);

            public LocalizedString this[string name, params object[] arguments]
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

                    translation = String.Format(translation, arguments);

                    return new LocalizedString(name, translation, false);
                }
            }

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
                => Enumerable.Empty<LocalizedString>();

            public IStringLocalizer WithCulture(CultureInfo culture) => Instance;

            LocalizedHtmlString IHtmlLocalizer.this[string name] => new LocalizedHtmlString(name, name, true);

            LocalizedHtmlString IHtmlLocalizer.this[string name, params object[] arguments]
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

            public LocalizedString GetString(string name) => this[name];

            public LocalizedString GetString(string name, params object[] arguments) => this[name, arguments];

            IHtmlLocalizer IHtmlLocalizer.WithCulture(CultureInfo culture) => Instance;
        }
    }
}
