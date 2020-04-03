using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Tests.Stubs
{
    public class NullStringLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name]
        {
            get
            {
                return new LocalizedString(name, "");
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                return new LocalizedString(name, "");
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return Enumerable.Empty<LocalizedString>();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return this;
        }
    }

    public class NullStringLocalizer<T> : NullStringLocalizer, IStringLocalizer<T>
    {
    }
}
