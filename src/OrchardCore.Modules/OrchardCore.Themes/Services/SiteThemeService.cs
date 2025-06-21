using OrchardCore.Environment.Extensions;
using OrchardCore.Settings;

namespace OrchardCore.Themes.Services;

public class SiteThemeService : ISiteThemeService
{
    private readonly ISiteService _siteService;
    private readonly IExtensionManager _extensionManager;

    public SiteThemeService(
        ISiteService siteService,
        IExtensionManager extensionManager)
    {
        _siteService = siteService;
        _extensionManager = extensionManager;
    }

    public async Task<IExtensionInfo> GetSiteThemeAsync()
    {
        var currentThemeName = await GetSiteThemeNameAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(currentThemeName))
        {
            return null;
        }

        return _extensionManager.GetExtension(currentThemeName);
    }

    public async Task SetSiteThemeAsync(string themeName)
    {
        var site = await _siteService.LoadSiteSettingsAsync().ConfigureAwait(false);
        site.Properties["CurrentThemeName"] = themeName;
        await _siteService.UpdateSiteSettingsAsync(site).ConfigureAwait(false);
    }

    public async Task<string> GetSiteThemeNameAsync()
    {
        var site = await _siteService.GetSiteSettingsAsync().ConfigureAwait(false);

        return (string)site.Properties["CurrentThemeName"];
    }
}
