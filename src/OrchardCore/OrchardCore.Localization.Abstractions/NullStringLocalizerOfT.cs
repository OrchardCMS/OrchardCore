using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    public class NullStringLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => NullStringLocalizer.Instance[name];

        public LocalizedString this[string name, params object[] arguments] => NullStringLocalizer.Instance[name, arguments];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => NullStringLocalizer.Instance.GetAllStrings(includeParentCultures);
    }
}
