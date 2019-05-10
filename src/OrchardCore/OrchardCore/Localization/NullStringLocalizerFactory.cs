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
            public static NullStringLocalizer Instance { get; } = new NullStringLocalizer();

            public LocalizedString this[string name] => new LocalizedString(name, name);

            public LocalizedString this[string name, params object[] arguments]
                => new LocalizedString(name, string.Format(name, arguments));

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
                => Enumerable.Empty<LocalizedString>();

            public IStringLocalizer WithCulture(CultureInfo culture) => Instance;
        }
    }
}
