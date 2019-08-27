using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// Provides the theme defined in the site configuration for the current scope (request).
    /// This selector provides AdminTheme as default or fallback for Account|Registration|ResetPassword
    /// controllers based on SiteSettings.
    /// The same <see cref="ThemeSelectorResult"/> is returned if called multiple times 
    /// during the same scope.
    /// </summary>
    public class UsersThemeSelector : IThemeSelector
    {
        private readonly ISiteService _siteService;
        private readonly IAdminThemeService _adminThemeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersThemeSelector(
            ISiteService siteService,
            IAdminThemeService adminThemeService,
            IHttpContextAccessor httpContextAccessor)
        {
            _siteService = siteService;
            _adminThemeService = adminThemeService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ThemeSelectorResult> GetThemeAsync()
        {
            var routeData = _httpContextAccessor.HttpContext.GetRouteData()?.Values;

            if (routeData == null)
            {
                return null;
            }

            if (routeData["area"]?.ToString() == "OrchardCore.Users")
            {
                bool useSiteTheme = false;

                switch (routeData["controller"]?.ToString())
                {
                    case "Account":
                        useSiteTheme = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>().UseSiteTheme;
                        break;
                    case "Registration":
                        useSiteTheme = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UseSiteTheme;
                        break;
                    case "ResetPassword":
                        useSiteTheme = (await _siteService.GetSiteSettingsAsync()).As<ResetPasswordSettings>().UseSiteTheme;
                        break;
                    default:
                        return null;
                }

                string adminThemeName = await _adminThemeService.GetAdminThemeNameAsync();

                if (String.IsNullOrEmpty(adminThemeName))
                {
                    return null;
                }

                return new ThemeSelectorResult
                {
                    Priority = useSiteTheme ? -100 : 100,
                    ThemeName = adminThemeName
                };
            }

            return null;
        }
    }
}
