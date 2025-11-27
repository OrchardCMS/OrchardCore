using System.Globalization;

namespace OrchardCore.Localization;

/// <summary>
/// Represents a contract for a localization service.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Returns the default culture of the site.
    /// </summary>
    Task<string> GetDefaultCultureAsync();

    /// <summary>
    /// Returns all the supported cultures of the site. It also contains the default culture.
    /// </summary>
    Task<string[]> GetSupportedCulturesAsync();

    /// <summary>
    /// Gets all cultures recognized by .NET, including culture aliases.
    /// </summary>
    IEnumerable<CultureInfo> GetAllCulturesAndAliases();
    /// <summary>
    /// Get whether to set the request culture to a parent culture in case the culture is not determined.
    /// </summary>
    bool FallBackToParentCultures { get; }
}
