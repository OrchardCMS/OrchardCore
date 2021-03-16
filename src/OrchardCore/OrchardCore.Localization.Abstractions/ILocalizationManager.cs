using System.Globalization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Contract to manage the localization.
    /// </summary>
    public interface ILocalizationManager
    {
        /// <summary>
        /// Retrieves a dictionary for a specified culture.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/>.</param>
        /// <returns>A <see cref="CultureDictionary"/> for the specified culture.</returns>
        CultureDictionary GetDictionary(CultureInfo culture);
    }
}
