using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Themes.Services;

public class ThemeTogglerService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISiteService _siteService;
    private readonly string _cookieName;

    public ThemeTogglerService(
        IHttpContextAccessor httpContextAccessor,
        ISiteService siteService,
        ShellSettings shellSettings,
        IOptions<ThemesCookieOptions> cookieOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _siteService = siteService;
        CurrentTenant = shellSettings.Name;
        _cookieName = cookieOptions.Value.AdminTheme;
    }

    public string CurrentTenant { get; }

    public async Task<string> CurrentTheme()
    {
        var adminSettings = await _siteService.GetSettingsAsync<AdminSettings>();

        if (adminSettings.DisplayThemeToggler)
        {
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(_cookieName, out var value)
                && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return "auto";
        }

        return "lite";
    }
}
