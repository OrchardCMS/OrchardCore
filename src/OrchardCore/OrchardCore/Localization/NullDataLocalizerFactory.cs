using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    internal class NullDataLocalizerFactory : IDataLocalizerFactory
    {
        public IDataLocalizer Create() => NullDataLocalizer.Instance;

        private class NullDataLocalizer : IDataLocalizer
        {
            public static NullDataLocalizer Instance { get; } = new NullDataLocalizer();

            public LocalizedString this[string name] => new LocalizedString(name, name);

            public LocalizedString this[string name, params object[] arguments]
                => new LocalizedString(name, String.Format(name, arguments));

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
                => Enumerable.Empty<LocalizedString>();

            public IStringLocalizer WithCulture(CultureInfo culture) => Instance;
        }
    }
}
