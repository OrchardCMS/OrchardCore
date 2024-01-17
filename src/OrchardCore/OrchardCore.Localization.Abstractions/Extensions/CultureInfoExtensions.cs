using System.Globalization;

namespace OrchardCore.Localization;

/// <summary>
/// Provides an extension methods for <see cref="CultureInfo"/> to deal with language direction.
/// </summary>
public static class CultureInfoExtensions
{
    /// <summary>
    /// Gets the language direction for a given culture.
    /// </summary>
    /// <param name="culture">The <see cref="CultureInfo"/>.</param>
    public static string GetLanguageDirection(this CultureInfo culture) => culture.IsRightToLeft()
        ? LanguageDirection.RTL
        : LanguageDirection.LTR;

    /// <summary>
    /// Gets whether the culture is RTL or not.
    /// </summary>
    /// <param name="culture">The culture.</param>
    public static bool IsRightToLeft(this CultureInfo culture) => culture.TextInfo.IsRightToLeft;
}
