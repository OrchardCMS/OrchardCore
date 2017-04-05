using Microsoft.Extensions.Caching.Memory;
using Orchard.Settings;
using Orchard.Environment.Extensions;
using System;
using System.Threading.Tasks;

namespace Orchard.UserCenter
{
    public class UserCenterThemeService : IUserCenterThemeService
    {
        private const string CacheKey = "UserCenterThemeName";

        private readonly IExtensionManager _extensionManager;
        private readonly ISiteService _siteService;
        private readonly IMemoryCache _memoryCache;

        public UserCenterThemeService(
            ISiteService siteService,
            IExtensionManager extensionManager,
            IMemoryCache memoryCache
            )
        {
            _siteService = siteService;
            _extensionManager = extensionManager;
            _memoryCache = memoryCache;
        }

        public async Task<IExtensionInfo> GetUserCenterThemeAsync()
        {
            string currentThemeName = await GetUserCenterThemeNameAsync();
            if (String.IsNullOrEmpty(currentThemeName))
            {
                return null;
            }

            return _extensionManager.GetExtension(currentThemeName);
        }

        public async Task SetUserCenterThemeAsync(string themeName)
        {
            var site = await _siteService.GetSiteSettingsAsync();
            site.Properties["CurrentUserCenterThemeName"] = themeName;
            //(site as IContent).ContentItem.Content.CurrentAdminThemeName = themeName;
            _memoryCache.Set(CacheKey, themeName);
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        public async Task<string> GetUserCenterThemeNameAsync()
        {
            string themeName;
            if (!_memoryCache.TryGetValue(CacheKey, out themeName))
            {
                var site = await _siteService.GetSiteSettingsAsync();
                themeName = (string)site.Properties["CurrentUserCenterThemeName"];
                // themeName = (string)(site as IContent).ContentItem.Content.CurrentAdminThemeName;
                _memoryCache.Set(CacheKey, themeName);
            }

            return themeName;
        }
    }
}
