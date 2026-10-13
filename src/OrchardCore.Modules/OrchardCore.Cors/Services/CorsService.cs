using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Cors.Settings;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Settings;

namespace OrchardCore.Cors.Services;

public class CorsService
{
    private readonly ISiteService _siteService;

    public CorsService(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async Task<CorsSettings> GetSettingsAsync()
    {
        return await _siteService.GetSettingsAsync<CorsSettings>();
    }

    internal async Task UpdateSettingsAsync(CorsSettings corsSettings)
    {
        var siteSettings = await _siteService.LoadSiteSettingsAsync();
        siteSettings.Put(nameof(CorsSettings), corsSettings);

        // This service is a singleton, so the scoped notifier is resolved from the current shell scope.
        ShellScope.Services.GetRequiredService<IOptionsUpdateNotifier>().RequestUpdate<CorsOptions>();

        await _siteService.UpdateSiteSettingsAsync(siteSettings);
    }
}
