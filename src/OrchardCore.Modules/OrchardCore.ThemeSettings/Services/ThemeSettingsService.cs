using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.ThemeSettings.Models;

namespace OrchardCore.ThemeSettings.Services
{
    public class ThemeSettingsService : IThemeSettingsService
    {
        private readonly ISiteService _siteService;

        public ThemeSettingsService(
            ISiteService siteService,
            IStringLocalizer<ThemeSettingsService> stringLocalizer)
        {
            _siteService = siteService;
        }

        public async Task<CustomThemeSettings> GetThemeSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            if (siteSettings.Has<CustomThemeSettings>())
            {
                return siteSettings.As<CustomThemeSettings>();
            }
            else
            {
                return new CustomThemeSettings();
            }
        }
    }
}
