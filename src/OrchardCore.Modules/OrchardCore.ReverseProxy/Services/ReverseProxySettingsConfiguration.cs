using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using OrchardCore.ReverseProxy.Settings;

namespace OrchardCore.ReverseProxy.Services;

public class ReverseProxySettingsConfiguration : IConfigureOptions<ReverseProxySettings>
{
    private readonly IReverseProxyService _reverseProxyService;

    public ReverseProxySettingsConfiguration(IReverseProxyService reverseProxyService)
    {
        _reverseProxyService = reverseProxyService;
    }

    public void Configure(ReverseProxySettings options)
    {
        var settings = GetReverseProxySettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (settings is not null)
        {
            options.ForwardedHeaders = settings.ForwardedHeaders;
            options.KnownNetworks = settings.KnownNetworks;
            options.KnownProxies = settings.KnownProxies;
        }
    }

    private async Task<ReverseProxySettings> GetReverseProxySettingsAsync()
    {
        var settings = await _reverseProxyService.GetSettingsAsync();

        if (_reverseProxyService.ValidateSettings(settings).Any(result => result != ValidationResult.Success))
        {
            return null;
        }

        return settings;
    }
}

