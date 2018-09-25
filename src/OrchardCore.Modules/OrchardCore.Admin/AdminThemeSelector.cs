using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Theming;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Admin
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
            IAdminThemeService adminThemeService,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _adminThemeService = adminThemeService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ThemeSelectorResult> GetThemeAsync()
        {
            string adminThemeName = await _adminThemeService.GetAdminThemeNameAsync();
            if (String.IsNullOrEmpty(adminThemeName))
            {
                return null;
            }

            var result = new ThemeSelectorResult()
            {
                ThemeName = adminThemeName,
                Priority = -1
            };

            if (AdminAttribute.IsApplied(_httpContextAccessor.HttpContext))
            {
                result.Priority = 100;
            }

            return result;
        }
    }
}
