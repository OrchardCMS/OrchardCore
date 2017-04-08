using Microsoft.AspNetCore.Http;
using Orchard.DisplayManagement.Theming;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
namespace Orchard.Admin
{
    /// <summary>
    /// Provides the theme defined in the site configuration for the current scope (request).
    /// The same <see cref="ThemeSelectorResult"/> is returned if called multiple times
    /// during the same scope.
    /// </summary>
    public class AdminThemeSelector : IThemeSelector
    {
        private readonly IAdminThemeService _adminThemeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminThemeSelector(
            IStringLocalizer<AdminThemeSelector>s,
            IAdminThemeService adminThemeService,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _adminThemeService = adminThemeService;
            _httpContextAccessor = httpContextAccessor;
            T = s;
        }

        public async Task<ThemeSelectorResult> GetThemeAsync()
        {
            if (AdminAttribute.IsApplied(_httpContextAccessor.HttpContext))
            {
                string adminThemeName = await _adminThemeService.GetAdminThemeNameAsync();
                if (String.IsNullOrEmpty(adminThemeName))
                {
                    return null;
                }

                return new ThemeSelectorResult
                {
                    Priority = 100,
                    ThemeName = adminThemeName
                };
            }

            return null;
        }
        IStringLocalizer T { get; set; }
        public bool CanSet { get { return true; } }
        public void SetTheme(string themeName)
        {
            _adminThemeService.SetAdminThemeAsync(themeName);
        }

        public string Tag { get { return "admin"; } }

        public LocalizedString DisplayName { get { return T["AdminTheme"]; } }

        public string Name { get { return "adminTheme"; } }
    }
}
