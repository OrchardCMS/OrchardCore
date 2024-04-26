using System;
using System.Threading.Tasks;

namespace OrchardCore.Localization;

/// <summary>
/// Contract to provide a translations.
/// </summary>
public interface ITranslationProvider
{
    /// <summary>
    /// Loads translations from a certain source for a specific culture.
    /// </summary>
    /// <param name="cultureName">The culture name.</param>
    /// <param name="dictionary">The <see cref="CultureDictionary"/> that will contains all loaded translations.</param>
    [Obsolete("This method has been deprecated, please use LoadTranslationsAsync instead.")]
    void LoadTranslations(string cultureName, CultureDictionary dictionary)
        => LoadTranslationsAsync(cultureName, dictionary).GetAwaiter().GetResult();

    /// <summary>
    /// Loads translations from a certain source for a specific culture.
    /// </summary>
    /// <param name="cultureName">The culture name.</param>
    /// <param name="dictionary">The <see cref="CultureDictionary"/> that will contains all loaded translations.</param>
    Task LoadTranslationsAsync(string cultureName, CultureDictionary dictionary);
}
