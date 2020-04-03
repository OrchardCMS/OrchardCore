using System.Globalization;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;

namespace OrchardCore.Localization.Services
{
    public class LocalizationService : ILocalizationService
    {
        private static readonly string DefaultCulture = CultureInfo.InstalledUICulture.Name;
        private static readonly string[] SupportedCultures = new[] { CultureInfo.InstalledUICulture.Name };

        private readonly ISiteService _siteService;
        private LocalizationSettings _localizationSettings;

        public LocalizationService(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<string> GetDefaultCultureAsync()
        {
            await InitializeLocalizationSettingsAsync();

            return _localizationSettings.DefaultCulture ?? DefaultCulture;
        }

        public async Task<string[]> GetSupportedCulturesAsync()
        {
            await InitializeLocalizationSettingsAsync();

            return _localizationSettings.SupportedCultures == null || _localizationSettings.SupportedCultures.Length == 0
                ? SupportedCultures
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
