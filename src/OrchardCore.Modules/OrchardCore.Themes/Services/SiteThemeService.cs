using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Extensions;
using OrchardCore.Settings;

namespace OrchardCore.Themes.Services
{
    public class SiteThemeService : ISiteThemeService
    {
        private const string CacheKey = "CurrentThemeName";

        private readonly ISiteService _siteService;
        private readonly IExtensionManager _extensionManager;
        private readonly IMemoryCache _memoryCache;

        public SiteThemeService(
            ISiteService siteService,
            IExtensionManager extensionManager,
            IMemoryCache memoryCache)
        {
            _siteService = siteService;
            _extensionManager = extensionManager;
            _memoryCache = memoryCache;
        }

        public async Task<IExtensionInfo> GetSiteThemeAsync()
        {
            string currentThemeName = await GetCurrentThemeNameAsync();
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

        public async Task<string> GetCurrentThemeNameAsync()
        {
            if (!_memoryCache.TryGetValue(CacheKey, out string themeName))
            {
                var changeToken = _siteService.ChangeToken;
                var site = await _siteService.GetSiteSettingsAsync();
                themeName = (string)site.Properties["CurrentThemeName"];
                _memoryCache.Set(CacheKey, themeName, changeToken);
            }

            return themeName;
        }
    }
}
