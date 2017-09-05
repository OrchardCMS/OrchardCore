using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Theming;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Themes.Services
{
    /// <summary>
    /// Provides the theme defined in the site configuration for the current scope (request).
    /// The same <see cref="ThemeSelectorResult"/> is returned if called multiple times 
    /// during the same scope.
    /// </summary>
    public class SiteThemeSelector : IThemeSelector
    {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SiteThemeSelector(
            ISiteThemeService siteThemeService, 
            IHttpContextAccessor httpContextAccessor)
        {
            _siteThemeService = siteThemeService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ThemeSelectorResult> GetThemeAsync()
        {
            string currentThemeName = await _siteThemeService.GetCurrentThemeNameAsync();
            if (String.IsNullOrEmpty(currentThemeName))
            {
                return null;
            }

            return new ThemeSelectorResult
            {
                Priority = 0,
                ThemeName = currentThemeName
            };
        }
    }
}
