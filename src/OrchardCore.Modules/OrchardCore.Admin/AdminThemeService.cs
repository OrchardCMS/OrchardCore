using System;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;
using OrchardCore.Settings;

namespace OrchardCore.Admin
{
    public class AdminThemeService : IAdminThemeService
    {
        private readonly ISiteService _siteService;
        private readonly IExtensionManager _extensionManager;

        public AdminThemeService(
            ISiteService siteService,
            IExtensionManager extensionManager)
        {
            _siteService = siteService;
            _extensionManager = extensionManager;
        }

        public async Task<IExtensionInfo> GetAdminThemeAsync()
        {
            string currentThemeName = await GetAdminThemeNameAsync();
            if (String.IsNullOrEmpty(currentThemeName))
            {
                return null;
            }

            return _extensionManager.GetExtension(currentThemeName);
        }

        public async Task SetAdminThemeAsync(string themeName)
        {
            var site = await _siteService.LoadSiteSettingsAsync();
            site.Properties["CurrentAdminThemeName"] = themeName;
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        public async Task<string> GetAdminThemeNameAsync()
        {
            var site = await _siteService.GetSiteSettingsAsync();
            return (string)site.Properties["CurrentAdminThemeName"];
        }
    }
}
