using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Settings.Services;

internal sealed class PreloadSiteSettingsTenantEventHandler : ModularTenantEvents
{
    private readonly ISiteService _siteService;
    private readonly ShellSettings _shellSettings;

    public PreloadSiteSettingsTenantEventHandler(ISiteService siteService, ShellSettings shellSettings)
    {
        _siteService = siteService;
        _shellSettings = shellSettings;
    }

    public override async Task ActivatedAsync()
    {
        if (_shellSettings.IsUninitialized())
        {
            // If the tenant is 'Uninitialized' there is no registered 'ISession' and then 'ISiteService' can't be used.
            return;
        }

        // Preload the site settings to ensure that the the required database access is asynchronous. This allows to
        // safely retrieve the site settings synchronously afterwards.
        _ = await _siteService.GetSiteSettingsAsync().ConfigureAwait(false);
    }
}
