using Microsoft.Extensions.Options;
using OrchardCore.ReverseProxy.Settings;

namespace OrchardCore.ReverseProxy.Services;

public class ReverseProxySettingsConfiguration : IConfigureOptions<ReverseProxySettings>
{
    private readonly ReverseProxyService _reverseProxyService;

    public ReverseProxySettingsConfiguration(ReverseProxyService reverseProxyService)
        => _reverseProxyService = reverseProxyService;

    public void Configure(ReverseProxySettings options)
    {
        var settings = _reverseProxyService.GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.ForwardedHeaders = settings.ForwardedHeaders;
        options.KnownNetworks = settings.KnownNetworks;
        options.KnownProxies = settings.KnownProxies;
    }
}

