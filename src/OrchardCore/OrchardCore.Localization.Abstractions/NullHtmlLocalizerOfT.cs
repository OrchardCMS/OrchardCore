using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Minimalistic HTML-aware localizer that does nothing.
    /// </summary>
    /// <typeparam name="T">The <see cref="System.Type"/> to provide strings for.</typeparam>
    public class NullHtmlLocalizer<T> : IHtmlLocalizer<T>
    {
        /// <inheritdoc/>
        public LocalizedHtmlString this[string name] => NullHtmlLocalizer.Instance[name];

        /// <inheritdoc/>
        public LocalizedHtmlString this[string name, params object[] arguments] => NullHtmlLocalizer.Instance[name, arguments];

        /// <inheritdoc/>
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => NullHtmlLocalizer.Instance.GetAllStrings(includeParentCultures);

        /// <inheritdoc/>
        public LocalizedString GetString(string name)
            => NullHtmlLocalizer.Instance.GetString(name);

        /// <inheritdoc/>
        public LocalizedString GetString(string name, params object[] arguments)
            => NullHtmlLocalizer.Instance.GetString(name, arguments);
    }
}
