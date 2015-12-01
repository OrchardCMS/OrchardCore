using Orchard.DisplayManagement.Theming;
using System;
using System.Threading.Tasks;

namespace Orchard.Themes.Services
{
    public class SiteThemeSelector : IThemeSelector
    {
        private readonly ISiteThemeService _siteThemeService;

        public SiteThemeSelector(ISiteThemeService siteThemeService)
        {
            _siteThemeService = siteThemeService;
        }

        public async Task<ThemeSelectorResult> GetThemeAsync()
        {
            string currentThemeName = await _siteThemeService.GetCurrentThemeNameAsync();
            if (String.IsNullOrEmpty(currentThemeName))
            {
                return null;
            }

            return new ThemeSelectorResult { Priority = -5, ThemeName = currentThemeName };
        }
    }
}
