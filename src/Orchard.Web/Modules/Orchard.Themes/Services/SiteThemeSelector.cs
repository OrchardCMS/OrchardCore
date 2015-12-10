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
        private ThemeSelectorResult cachedSelectorResult;

        public SiteThemeSelector(ISiteThemeService siteThemeService)
        {
            _siteThemeService = siteThemeService;
        }

        public async Task<ThemeSelectorResult> GetThemeAsync()
        {
            if (cachedSelectorResult == null)
            {
                string currentThemeName = await _siteThemeService.GetCurrentThemeNameAsync();
                if (String.IsNullOrEmpty(currentThemeName))
                {
                    return null;
                }

                cachedSelectorResult = new ThemeSelectorResult
                {
                    Priority = -5,
                    ThemeName = currentThemeName
                };
            }

            return cachedSelectorResult;            
        }
    }
}
