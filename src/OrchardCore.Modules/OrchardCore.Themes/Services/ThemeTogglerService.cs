using Microsoft.AspNetCore.Http;
using OrchardCore.Admin.Models;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Themes.Services;

public class ThemeTogglerService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISiteService _siteService;

    public ThemeTogglerService(
        IHttpContextAccessor httpContextAccessor,
        ISiteService siteService,
        ShellSettings shellSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _siteService = siteService;
        CurrentTenant = shellSettings.Name;
    }

    public string CurrentTenant { get; }

    public async Task<string> CurrentTheme()
    {
        var adminSettings = await _siteService.GetSettingsAsync<AdminSettings>();

        if (adminSettings.DisplayThemeToggler)
        {
            var cookieName = $"{CurrentTenant}-admintheme";

            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(cookieName, out var value)
                && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return "auto";
        }

        return "lite";
    }
}
