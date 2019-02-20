using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Theming;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// Provides the theme defined in the site configuration for the current scope (request).
    /// This selector provides AdminTheme as fallback for Account|Registration|ResetPassword controllers.
    /// The same <see cref="ThemeSelectorResult"/> is returned if called multiple times 
    /// during the same scope.
    /// </summary>
    public class UsersThemeSelector : IThemeSelector
    {
        private readonly IAdminThemeService _adminThemeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static string[] handledControllers = new string[] {
            "Account",
            "Registration",
            "ResetPassword"
        };

        public UsersThemeSelector(
            IAdminThemeService adminThemeService,
            IHttpContextAccessor httpContextAccessor)
        {
            _adminThemeService = adminThemeService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ThemeSelectorResult> GetThemeAsync()
        {
            var routeData = _httpContextAccessor.HttpContext.GetRouteData().Values;

            if (routeData["area"]?.ToString() == "OrchardCore.Users" && handledControllers.Contains(routeData["controller"]?.ToString()))
            {
                string adminThemeName = await _adminThemeService.GetAdminThemeNameAsync();
                if (String.IsNullOrEmpty(adminThemeName))
                {
                    return null;
                }

                return new ThemeSelectorResult
                {
                    Priority = -100,    // Don't override SiteThemeSelector
                    ThemeName = adminThemeName
                };
            }

            return null;
        }
    }
}
