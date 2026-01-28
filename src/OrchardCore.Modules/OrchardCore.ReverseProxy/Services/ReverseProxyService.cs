using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.Settings;

namespace OrchardCore.ReverseProxy.Services;

public class ReverseProxyService : IReverseProxyService
{
    private readonly ISiteService _siteService;

    protected readonly IStringLocalizer S;

    public ReverseProxyService(ISiteService siteService, IStringLocalizer<ReverseProxyService> stringLocalizer)
    {
        _siteService = siteService;
        S = stringLocalizer;
    }

    public Task<ReverseProxySettings> GetSettingsAsync()
        => _siteService.GetSettingsAsync<ReverseProxySettings>();

    public async Task<ReverseProxySettings> LoadSettingsAsync()
    {
        var siteSettings = await _siteService.LoadSiteSettingsAsync();

        return siteSettings.As<ReverseProxySettings>();
    }

    public async Task UpdateSettingsAsync(ReverseProxySettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var siteSettings = await _siteService.LoadSiteSettingsAsync();

        siteSettings.Alter<ReverseProxySettings>(aspect =>
        {
            aspect.ForwardedHeaders = settings.ForwardedHeaders;
            aspect.KnownNetworks = settings.KnownNetworks;
            aspect.KnownProxies = settings.KnownProxies;
        });

        await _siteService.UpdateSiteSettingsAsync(siteSettings);
    }

    public IEnumerable<ValidationResult> ValidateSettings(ReverseProxySettings settings)
    {
        return [];
    }
}
