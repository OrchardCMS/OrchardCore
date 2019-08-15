using System.Globalization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Provides an extensions methods for <see cref="CultureInfo"/>.
    /// </summary>
    public static class CultureInfoExtensions
    {
        /// <summary>
        /// Gets the language direction for a given culture.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/>.</param>
        /// <returns></returns>
        public static string GetLanguageDirection(this CultureInfo culture)
            => culture.TextInfo.IsRightToLeft ? LanguageDirection.RTL : LanguageDirection.LTR;

        /// <summary>
        /// Gets whether the culture is support RTL or not.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static bool IsRightToLeft(this CultureInfo culture)
            => culture.TextInfo.IsRightToLeft;
    }
}