using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Extensions;
using OrchardCore.Settings;

namespace OrchardCore.Themes.Services
{
    public class SiteThemeService : ISiteThemeService
    {
        private const string CacheKey = "CurrentThemeName";

        private readonly IExtensionManager _extensionManager;
        private readonly ISiteService _siteService;

        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        public SiteThemeService(
            ISiteService siteService,
            IExtensionManager extensionManager,
            IMemoryCache memoryCache,
            ISignal signal)
        {
            _siteService = siteService;
            _extensionManager = extensionManager;
            _memoryCache = memoryCache;
            _signal = signal;
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
            var site = await _siteService.GetSiteSettingsAsync();
            site.Properties["CurrentThemeName"] = themeName;
            //(site as IContent).ContentItem.Content.CurrentThemeName = themeName;
            await _signal.SignalTokenAsync(CacheKey);
            _memoryCache.Set(CacheKey, themeName, _signal.GetToken(CacheKey));
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        public async Task<string> GetCurrentThemeNameAsync()
        {
            string themeName;
            if (!_memoryCache.TryGetValue(CacheKey, out themeName))
            {
                var site = await _siteService.GetSiteSettingsAsync();
                themeName = (string)site.Properties["CurrentThemeName"];
                _memoryCache.Set(CacheKey, themeName, _signal.GetToken(CacheKey));
            }

            return themeName;
        }
    }
}
