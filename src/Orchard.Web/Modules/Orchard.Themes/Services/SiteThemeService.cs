using Orchard.Core.Settings.Services;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using System;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Themes.Services
{
    public class SiteThemeService : ISiteThemeService
    {
        public const string CurrentThemeSignal = "SiteCurrentTheme";

        private readonly IExtensionManager _extensionManager;
        private readonly ISiteService _siteService;
        private readonly ISession _session;

        // TODO: Implement caching
        //private readonly ICacheManager _cacheManager;
        //private readonly ISignals _signals;

        public SiteThemeService(
            ISiteService siteService,
            IExtensionManager extensionManager,
            ISession session)
        {
            _siteService = siteService;
            _extensionManager = extensionManager;
            _session = session;
        }

        public async Task<ExtensionDescriptor> GetSiteThemeAsync()
        {
            string currentThemeName = await GetCurrentThemeNameAsync();
            if (String.IsNullOrEmpty(currentThemeName))
            {
                return null;
            }

            return _extensionManager.GetExtension(currentThemeName);
        }

        public async Task<ExtensionDescriptor> GetAdminThemeAsync()
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
            site.ContentItem.Content.CurrentThemeName = themeName;
            _session.Save(site.ContentItem);
        }

        public async Task SetAdminThemeAsync(string themeName)
        {
            var site = await _siteService.GetSiteSettingsAsync();
            site.ContentItem.Content.CurrentAdminThemeName = themeName;
            _session.Save(site.ContentItem);
        }

        public async Task<string> GetCurrentThemeNameAsync()
        {
            var site = await _siteService.GetSiteSettingsAsync();
            return (string)site.ContentItem.Content.CurrentThemeName;
        }

        public async Task<string> GetAdminThemeNameAsync()
        {
            var site = await _siteService.GetSiteSettingsAsync();
            return (string)site.ContentItem.Content.CurrentAdminThemeName;
        }
    }
}
