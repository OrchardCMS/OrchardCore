using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Extensions;
using OrchardCore.Settings;

namespace OrchardCore.Admin
{
    public class AdminThemeService : IAdminThemeService
    {
        private const string CacheKey = "AdminThemeName";

        private readonly ISiteService _siteService;
        private readonly IExtensionManager _extensionManager;
        private readonly IMemoryCache _memoryCache;

        //creating of new special class variable
        public AdminThemeService(
            ISiteService siteService,
            IExtensionManager extensionManager,
            IMemoryCache memoryCache)
        {
            _siteService = siteService;
            _extensionManager = extensionManager;
            _memoryCache = memoryCache;
        }

        public async Task<IExtensionInfo> GetAdminThemeAsync()
        {
            //theme name take admin theme name
            string currentThemeName = await GetAdminThemeNameAsync();
             //what to do if there are no current theme name
            if (String.IsNullOrEmpty(currentThemeName))
            {
                return null;
            }

            return _extensionManager.GetExtension(currentThemeName);
        }
        //enable to set admin theme name
        public async Task SetAdminThemeAsync(string themeName)
        {
            var site = await _siteService.LoadSiteSettingsAsync();
            site.Properties["CurrentAdminThemeName"] = themeName;
            await _siteService.UpdateSiteSettingsAsync(site);
        }
         //program get and save theme name
        public async Task<string> GetAdminThemeNameAsync()
        {
            string themeName;
            if (!_memoryCache.TryGetValue(CacheKey, out themeName))
            {
                var changeToken = _siteService.ChangeToken;
                var site = await _siteService.GetSiteSettingsAsync();
                themeName = (string)site.Properties["CurrentAdminThemeName"];
                _memoryCache.Set(CacheKey, themeName, changeToken);
            }

            return themeName;
        }
    }
}
