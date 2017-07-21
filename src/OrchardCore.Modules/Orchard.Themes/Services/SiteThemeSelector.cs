using Microsoft.AspNetCore.Http;
using Orchard.DisplayManagement.Theming;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SiteThemeSelector(
            IStringLocalizer<SiteThemeSelector> t,
            ISiteThemeService siteThemeService, 
            IHttpContextAccessor httpContextAccessor)
        {
            _siteThemeService = siteThemeService;
            _httpContextAccessor = httpContextAccessor;
            T = t;
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
        IStringLocalizer T { get; set; }
        public bool CanSet { get { return true; } }
        public void SetTheme(string themeName)
        {
            _siteThemeService.SetSiteThemeAsync(themeName);
        }

        public string Tag { get { return "site"; } }

        public LocalizedString DisplayName { get { return T["SiteTheme"]; } }

        public string Name { get { return "siteTheme"; } }
    }
}
