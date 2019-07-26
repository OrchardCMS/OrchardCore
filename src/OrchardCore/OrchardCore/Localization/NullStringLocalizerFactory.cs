using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    internal class NullStringLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource) => NullStringLocalizer.Instance;
        public IStringLocalizer Create(string baseName, string location) => NullStringLocalizer.Instance;

        private class NullStringLocalizer : IStringLocalizer
        {
            private static readonly PluralizationRuleDelegate _defaultPluralRule = n => (n == 1) ? 0 : 1;

            public static NullStringLocalizer Instance { get; } = new NullStringLocalizer();

            public LocalizedString this[string name] => new LocalizedString(name, name);

            public LocalizedString this[string name, params object[] arguments]
            {
                get
                {
                    var value = name;
                    if (arguments.Length == 1 && arguments[0] is PluralizationArgument pluralArgument)
                    {
                        var pluralForm = pluralArgument.Forms[_defaultPluralRule(pluralArgument.Count)];
                        value = String.Format(pluralForm, pluralArgument.Count);
                    }

                    return new LocalizedString(name, value);
                }
            }

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
                => Enumerable.Empty<LocalizedString>();

            public IStringLocalizer WithCulture(CultureInfo culture) => Instance;
        }
    }
}
