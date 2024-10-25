using Microsoft.AspNetCore.Http;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

/// <summary>
/// Provides the theme defined in the site configuration for the current scope (request).
/// This selector provides AdminTheme as default or fallback for Account for Registration,
/// ResetPassword, TwoFactorAuthentication, SmsAuthenticator and AuthenticatorApp
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
        var routeValues = _httpContextAccessor.HttpContext.Request.RouteValues;

        if (routeValues["area"]?.ToString() == UserConstants.Features.Users)
        {
            bool useSiteTheme;

            switch (routeValues["controller"]?.ToString())
            {
                case "Account":
                case "ExternalAuthentications":
                    useSiteTheme = (await _siteService.GetSettingsAsync<LoginSettings>()).UseSiteTheme;
                    break;
                case "TwoFactorAuthentication":
                    {
                        if (routeValues["action"] != null
                           && routeValues["action"].ToString().StartsWith("LoginWith", StringComparison.OrdinalIgnoreCase)
                           && (await _siteService.GetSettingsAsync<LoginSettings>()).UseSiteTheme)
                        {
                            useSiteTheme = true;
                        }
                        else
                        {
                            useSiteTheme = (await _siteService.GetSettingsAsync<TwoFactorLoginSettings>()).UseSiteTheme;
                        }
                    }
                    break;
                case "SmsAuthenticator":
                case "AuthenticatorApp":
                    {
                        useSiteTheme = (await _siteService.GetSettingsAsync<TwoFactorLoginSettings>()).UseSiteTheme;
                    }
                    break;
                case "Registration":
                    useSiteTheme = (await _siteService.GetSettingsAsync<RegistrationSettings>()).UseSiteTheme;
                    break;
                case "ResetPassword":
                    useSiteTheme = (await _siteService.GetSettingsAsync<ResetPasswordSettings>()).UseSiteTheme;
                    break;
                default:
                    return null;
            }

            var adminThemeName = await _adminThemeService.GetAdminThemeNameAsync();

            if (string.IsNullOrEmpty(adminThemeName))
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
