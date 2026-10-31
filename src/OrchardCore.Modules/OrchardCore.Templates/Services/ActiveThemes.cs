using OrchardCore.Admin;
using OrchardCore.Themes.Services;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Gets the active site theme, which the templates override, and the active admin theme, which the admin templates override.
/// </summary>
public sealed class ActiveThemes
{
    // Registered by the Themes and Admin features, which can be disabled.
    private readonly ISiteThemeService _siteThemeService;
    private readonly IAdminThemeService _adminThemeService;

    public ActiveThemes(IEnumerable<ISiteThemeService> siteThemeServices, IEnumerable<IAdminThemeService> adminThemeServices)
    {
        _siteThemeService = siteThemeServices.LastOrDefault();
        _adminThemeService = adminThemeServices.LastOrDefault();
    }

    /// <summary>
    /// Gets the identifier of the active theme.
    /// </summary>
    /// <param name="adminTemplates">Whether to get the admin theme, rather than the site theme.</param>
    /// <returns>The theme identifier, or <see langword="null"/> when there is no active theme or its feature is disabled.</returns>
    public async Task<string> GetThemeIdAsync(bool adminTemplates)
    {
        if (adminTemplates)
        {
            return _adminThemeService == null ? null : await _adminThemeService.GetAdminThemeNameAsync();
        }

        return _siteThemeService == null ? null : await _siteThemeService.GetSiteThemeNameAsync();
    }
}
