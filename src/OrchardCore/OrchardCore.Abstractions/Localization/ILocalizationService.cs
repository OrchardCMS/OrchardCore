using System.Globalization;

namespace OrchardCore.Localization;

/// <summary>
/// Represents a contract for a localization service.
/// </summary>
public interface ILocalizationService
{
    private static readonly CultureInfo[] _cultureAliases =
    [
        CultureInfo.GetCultureInfo("zh-CN"),
        CultureInfo.GetCultureInfo("zh-TW")
    ];

    /// <summary>
    /// Returns the default culture of the site.
    /// </summary>
    Task<string> GetDefaultCultureAsync();

    /// <summary>
    /// Returns all the supported cultures of the site. It also contains the default culture.
    /// </summary>
    Task<string[]> GetSupportedCulturesAsync();

    /// <summary>
    /// Get whether to set the request culture to a parent culture in case the culture is not determined.
    /// </summary>
    bool FallBackToParentCultures { get; }

    [Obsolete("This method is deprecated and will be removed in a future version. Use CultureInfoWrapper.GetAllCulturesAndAliases() instead.")]
    static CultureInfo[] GetAllCulturesAndAliases()
    {
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Union(_cultureAliases)
            .OrderBy(c => c.Name);

        return cultures.ToArray();
    }
}
