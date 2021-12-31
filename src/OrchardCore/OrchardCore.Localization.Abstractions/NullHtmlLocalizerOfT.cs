using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    public class NullHtmlLocalizer<T> : IHtmlLocalizer<T>
    {
        public LocalizedHtmlString this[string name] => NullHtmlLocalizer.Instance[name];

        public LocalizedHtmlString this[string name, params object[] arguments] => NullHtmlLocalizer.Instance[name, arguments];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => NullHtmlLocalizer.Instance.GetAllStrings(includeParentCultures);

        public LocalizedString GetString(string name)
            => NullHtmlLocalizer.Instance.GetString(name);

        public LocalizedString GetString(string name, params object[] arguments)
            => NullHtmlLocalizer.Instance.GetString(name, arguments);
    }
}
