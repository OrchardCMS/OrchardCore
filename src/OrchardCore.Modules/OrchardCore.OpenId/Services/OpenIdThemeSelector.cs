using Microsoft.AspNetCore.Http;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.OpenId.Services;

public sealed class OpenIdThemeSelector : IThemeSelector
{
    private readonly ISiteService _siteService;
    private readonly IAdminThemeService _adminThemeService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OpenIdThemeSelector(ISiteService siteService, IAdminThemeService adminThemeService, IHttpContextAccessor httpContextAccessor)
    {
        _siteService = siteService;
        _adminThemeService = adminThemeService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ThemeSelectorResult> GetThemeAsync()
    {
        var routeValues = _httpContextAccessor.HttpContext?.Request.RouteValues;
        if (routeValues?["area"]?.ToString() != OpenIdConstants.Features.Core
            || routeValues["controller"]?.ToString() != "Access")
        {
            return null;
        }

        var adminTheme = await _adminThemeService.GetAdminThemeNameAsync();
        if (string.IsNullOrEmpty(adminTheme))
        {
            return null;
        }

        // Match the login page: prefer its admin theme by default, or use it
        // only as a fallback when the site-theme option is enabled.
        var settings = await _siteService.GetSettingsAsync<LoginSettings>();
        return new ThemeSelectorResult
        {
            ThemeName = adminTheme,
            Priority = settings.UseSiteTheme ? -100 : 100,
        };
    }
}
