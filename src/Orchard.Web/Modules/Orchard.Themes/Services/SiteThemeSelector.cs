using Microsoft.AspNet.Http;
using Orchard.DisplayManagement.Admin;
using Orchard.DisplayManagement.Theming;
using System;
using System.Threading.Tasks;

namespace Orchard.Themes.Services
{
    /// <summary>
    /// Provides the theme defined in the site configuration for the current scope (request).
    /// The same <see cref="ThemeSelectorResult"/> is returned if called multiple times 
    /// during the same scope.
    /// </summary>
    public class SiteThemeSelector : IThemeSelector
    {
        private readonly ISiteThemeService _siteThemeService;
        private ThemeSelectorResult cachedSiteSelectorResult;
        private ThemeSelectorResult cachedAdminSelectorResult;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SiteThemeSelector(ISiteThemeService siteThemeService, IHttpContextAccessor httpContextAccessor)
        {
            _siteThemeService = siteThemeService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ThemeSelectorResult> GetThemeAsync()
        {
            if (AdminAttribute.IsApplied(_httpContextAccessor.HttpContext))
            {
                if (cachedAdminSelectorResult == null)
                {
                    string adminThemeName = await _siteThemeService.GetAdminThemeNameAsync();
                    if (String.IsNullOrEmpty(adminThemeName))
                    {
                        return null;
                    }

                    cachedAdminSelectorResult = new ThemeSelectorResult
                    {
                        Priority = -5,
                        ThemeName = adminThemeName
                    };

                    return cachedAdminSelectorResult;
                }
            }

            if (cachedSiteSelectorResult == null)
            {
                string currentThemeName = await _siteThemeService.GetCurrentThemeNameAsync();
                if (String.IsNullOrEmpty(currentThemeName))
                {
                    return null;
                }

                cachedSiteSelectorResult = new ThemeSelectorResult
                {
                    Priority = -5,
                    ThemeName = currentThemeName
                };

                return cachedSiteSelectorResult;
            }

            return null;
        }
    }
}
