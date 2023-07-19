using System.Globalization;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;

namespace OrchardCore.Localization.Services
{
    /// <summary>
    /// Represents a localization service.
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private static readonly string _defaultCulture = CultureInfo.InstalledUICulture.Name;
        private static readonly string[] _supportedCultures = new[] { CultureInfo.InstalledUICulture.Name };

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
            await InitializeLocalizationSettingsAsync();

            return _localizationSettings.DefaultCulture ?? _defaultCulture;
        }

        /// <inheritdocs />
        public async Task<string[]> GetSupportedCulturesAsync()
        {
            await InitializeLocalizationSettingsAsync();

            return _localizationSettings.SupportedCultures == null || _localizationSettings.SupportedCultures.Length == 0
                ? _supportedCultures
                : _localizationSettings.SupportedCultures
                ;
        }

        private async Task InitializeLocalizationSettingsAsync()
        {
            if (_localizationSettings == null)
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                _localizationSettings = siteSettings.As<LocalizationSettings>();
            }
        }
    }
}
