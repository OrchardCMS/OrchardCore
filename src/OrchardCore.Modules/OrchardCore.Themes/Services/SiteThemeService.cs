using System;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;
using OrchardCore.Settings;

namespace OrchardCore.Themes.Services
{
    public class SiteThemeService : ISiteThemeService
    {
        private readonly ISiteService _siteService;
        private readonly IExtensionManager _extensionManager;

        public SiteThemeService(
            ISiteService siteService,
            IExtensionManager extensionManager)
        {
            _siteService = siteService;
            _extensionManager = extensionManager;
        }

        public async Task<IExtensionInfo> GetSiteThemeAsync()
        {
            string currentThemeName = await GetSiteThemeNameAsync();
            if (String.IsNullOrEmpty(currentThemeName))
            {
                return null;
            }

            return _extensionManager.GetExtension(currentThemeName);
        }

        public async Task SetSiteThemeAsync(string themeName)
        {
            var site = await _siteService.LoadSiteSettingsAsync();
            site.Properties["CurrentThemeName"] = themeName;
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        public async Task<string> GetSiteThemeNameAsync()
        {
            var site = await _siteService.GetSiteSettingsAsync();

            return (string)site.Properties["CurrentThemeName"];
        }
    }
}
