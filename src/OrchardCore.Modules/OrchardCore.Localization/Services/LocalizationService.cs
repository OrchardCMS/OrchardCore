using System.Globalization;
using System.Threading.Tasks;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;

namespace OrchardCore.Localization.Services;

/// <summary>
/// Represents a localization service.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private static readonly string _defaultCulture = CultureInfo.InstalledUICulture.Name;
    private static readonly string[] _supportedCultures = [CultureInfo.InstalledUICulture.Name];

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
    public async Task<string> GetDefaultCultureAsync()
    {
        var settings = await GetLocalizationSettingsAsync();

        return settings.DefaultCulture ?? _defaultCulture;
    }

    /// <inheritdocs />
    public async Task<string[]> GetSupportedCulturesAsync()
    {
        var settings = await GetLocalizationSettingsAsync();

        return settings.SupportedCultures?.Length == 0
            ? _supportedCultures
            : settings.SupportedCultures;
    }

    private async Task<LocalizationSettings> GetLocalizationSettingsAsync()
    {
        if (_localizationSettings == null)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            _localizationSettings = siteSettings.As<LocalizationSettings>();
        }

        return _localizationSettings;
    }
}
