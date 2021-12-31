using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Minimalistic localizer that does nothing.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to provide strings for.</typeparam>
    public class NullStringLocalizer<T> : IStringLocalizer<T>
    {
        /// <inheritdoc/>
        public LocalizedString this[string name] => NullStringLocalizer.Instance[name];

        /// <inheritdoc/>
        public LocalizedString this[string name, params object[] arguments] => NullStringLocalizer.Instance[name, arguments];

        /// <inheritdoc/>
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => NullStringLocalizer.Instance.GetAllStrings(includeParentCultures);

        /// <inheritdoc/>
        [Obsolete("This method will be removed in the upcoming ASP.NET Core major release.")]
        public IStringLocalizer WithCulture(CultureInfo culture)
            => NullStringLocalizer.Instance.WithCulture(culture);
    }
}
