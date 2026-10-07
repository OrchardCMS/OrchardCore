using System.Globalization;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;

namespace OrchardCore.Localization.Services;

/// <summary>
/// Represents a localization service.
/// </summary>
public class LocalizationService : ILocalizationService
{
    // CultureInfo.InstalledUICulture.Name is empty when the OS/runtime has no configured
    // UI culture (e.g. invariant globalization mode, or a POSIX "C"/"C.UTF-8" locale, common
    // on Linux CI runners and containers) - falling back to "en" keeps GetSupportedCulturesAsync
    // from ever returning an empty culture name, which callers (e.g.
    // CultureInfo.GetCultureInfo(cultureName)) treat as invalid input and throw on.
    private static readonly string s_defaultCulture = string.IsNullOrEmpty(CultureInfo.InstalledUICulture.Name)
        ? "en"
        : CultureInfo.InstalledUICulture.Name;
    private static readonly string[] s_supportedCultures = [s_defaultCulture];

    private static readonly CultureInfo[] s_cultureAliases =
    [
        CultureInfo.GetCultureInfo("zh-CN"),
        CultureInfo.GetCultureInfo("zh-TW")
    ];

    private readonly ISiteService _siteService;

    private LocalizationSettings _localizationSettings;

    /// <summary>
    /// Creates a new instance of <see cref="LocalizationService"/>.
    /// </summary>
    /// <param name="siteService">The <see cref="ISiteService"/>.</param>
    public LocalizationService(ISiteService siteService)
    {
        _siteService = siteService;
    }

    /// <inheritdocs />
    public bool FallBackToParentCultures => _localizationSettings.FallBackToParentCulture;

    /// <inheritdocs />
    public async Task<string> GetDefaultCultureAsync()
    {
        await InitializeLocalizationSettingsAsync();

        return _localizationSettings.DefaultCulture ?? s_defaultCulture;
    }

    /// <inheritdocs />
    public async Task<string[]> GetSupportedCulturesAsync()
    {
        await InitializeLocalizationSettingsAsync();

        return _localizationSettings.SupportedCultures == null || _localizationSettings.SupportedCultures.Length == 0
            ? s_supportedCultures
            : _localizationSettings.SupportedCultures
            ;
    }

    /// <inheritdocs />
    public IEnumerable<CultureInfo> GetAllCulturesAndAliases()
    {
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Union(s_cultureAliases)
            .OrderBy(c => c.Name);

        return cultures;
    }

    private async Task InitializeLocalizationSettingsAsync()
    {
        _localizationSettings ??= await _siteService.GetSettingsAsync<LocalizationSettings>();
    }
}
