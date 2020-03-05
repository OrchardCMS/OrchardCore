using System.Globalization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Provides a language directions and helper methods that extend <see cref="CultureInfo"/>.
    /// </summary>
    public static class LanguageDirection
    {
        /// <summary>
        /// Defines left to right direction.
        /// </summary>
        public static readonly string LTR = "ltr";

        /// <summary>
        /// Defines right to left direction.
        /// </summary>
        public static readonly string RTL = "rtl";

        /// <summary>
        /// Gets the language direction for a given culture.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/>.</param>
        /// <returns></returns>
        public static string GetLanguageDirection(this CultureInfo culture)
            => culture.TextInfo.IsRightToLeft ? RTL : LTR;

        /// <summary>
        /// Gets whether the culture is RTL or not.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static bool IsRightToLeft(this CultureInfo culture)
            => culture.TextInfo.IsRightToLeft;
    }
}
