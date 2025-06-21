using OrchardCore.Environment.Extensions;
using OrchardCore.Settings;

namespace OrchardCore.Admin;

public class AdminThemeService : IAdminThemeService
{
    private readonly ISiteService _siteService;
    private readonly IExtensionManager _extensionManager;

    public AdminThemeService(
        ISiteService siteService,
        IExtensionManager extensionManager)
    {
        _siteService = siteService;
        _extensionManager = extensionManager;
    }

    public async Task<IExtensionInfo> GetAdminThemeAsync()
    {
        var currentThemeName = await GetAdminThemeNameAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(currentThemeName))
        {
            return null;
        }

        return _extensionManager.GetExtension(currentThemeName);
    }

    public async Task SetAdminThemeAsync(string themeName)
    {
        var site = await _siteService.LoadSiteSettingsAsync().ConfigureAwait(false);
        site.Properties["CurrentAdminThemeName"] = themeName;
        await _siteService.UpdateSiteSettingsAsync(site).ConfigureAwait(false);
    }

    public async Task<string> GetAdminThemeNameAsync()
    {
        var site = await _siteService.GetSiteSettingsAsync().ConfigureAwait(false);
        return (string)site.Properties["CurrentAdminThemeName"];
    }
}
