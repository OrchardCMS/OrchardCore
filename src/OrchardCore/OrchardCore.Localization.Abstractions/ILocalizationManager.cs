using System;
using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Localization;

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
    [Obsolete("This method has been deprecated, please use GetDictionaryAsync instead.")]
    CultureDictionary GetDictionary(CultureInfo culture) => GetDictionaryAsync(culture).GetAwaiter().GetResult();

    /// <summary>
    /// Retrieves a dictionary for a specified culture.
    /// </summary>
    /// <param name="culture">The <see cref="CultureInfo"/>.</param>
    /// <returns>A <see cref="CultureDictionary"/> for the specified culture.</returns>
    Task<CultureDictionary> GetDictionaryAsync(CultureInfo culture);
}
