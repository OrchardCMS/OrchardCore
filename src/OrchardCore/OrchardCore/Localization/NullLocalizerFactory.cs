using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    internal class NullLocalizerFactory : IStringLocalizerFactory, IHtmlLocalizerFactory
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
                    var value = String.Empty;
                    if (arguments.Length == 1 && arguments[0] is PluralizationArgument pluralArgument)
                    {
                        var pluralForm = pluralArgument.Forms[_defaultPluralRule(pluralArgument.Count)];
                        value = String.Format(pluralForm, pluralArgument.Count);
                    }
                    else
                    {
                        value = String.Format(name, arguments);
                    }

                    return new LocalizedString(name, value);
                }
            }

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
                => Enumerable.Empty<LocalizedString>();

            public IStringLocalizer WithCulture(CultureInfo culture) => Instance;

            LocalizedHtmlString IHtmlLocalizer.this[string name] => new LocalizedHtmlString(name, name);

            LocalizedHtmlString IHtmlLocalizer.this[string name, params object[] arguments]
                => new LocalizedHtmlString(name, this[name, arguments]);

            public LocalizedString GetString(string name) => this[name];

            public LocalizedString GetString(string name, params object[] arguments) => this[name, arguments];

            IHtmlLocalizer IHtmlLocalizer.WithCulture(CultureInfo culture) => Instance;
        }
    }
}
