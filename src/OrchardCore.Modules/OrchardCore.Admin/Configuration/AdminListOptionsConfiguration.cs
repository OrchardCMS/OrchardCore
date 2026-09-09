using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.Settings;

namespace OrchardCore.Admin.Configuration;

/// <summary>
/// Applies the admin list choices stored in the site settings on top of the values bound from
/// <c>appsettings.json</c>, so <see cref="IOptionsMonitor{TOptions}"/> always exposes the effective defaults.
/// </summary>
/// <remarks>
/// This runs after the <c>OrchardCore:AdminList</c> configuration section is bound, so what an administrator
/// picks in <b>Configuration → Settings → Admin</b> wins over the value configured for the tenant. The monitor
/// cache is refreshed by <see cref="Environment.Options.IOptionsUpdateNotifier"/> once the settings
/// update commits.
/// </remarks>
public sealed class AdminListOptionsConfiguration : IConfigureOptions<AdminListOptions>
{
    private readonly ISiteService _siteService;

    public AdminListOptionsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(AdminListOptions options)
    {
        var settings = _siteService.GetSettings<AdminSettings>();

        if (settings is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(settings.ListLayout))
        {
            options.DefaultLayout = settings.ListLayout.Trim();
        }

        if (!string.IsNullOrWhiteSpace(settings.ListActionsLayout))
        {
            options.DefaultActionsLayout = settings.ListActionsLayout.Trim();
        }
    }
}
