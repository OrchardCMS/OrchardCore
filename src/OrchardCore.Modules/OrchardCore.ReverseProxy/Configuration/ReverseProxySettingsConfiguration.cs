using Microsoft.Extensions.Options;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.Settings;

namespace OrchardCore.ReverseProxy.Configuration;

public class ReverseProxySettingsConfiguration : IConfigureOptions<ReverseProxySettings>
{
    private readonly ISiteService _siteService;

    public ReverseProxySettingsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(ReverseProxySettings options)
    {
        var settings = _siteService.GetSettings<ReverseProxySettings>();

        if (settings is not null)
        {
            options.ForwardedHeaders = settings.ForwardedHeaders;
            options.KnownNetworks = settings.KnownNetworks;
            options.KnownProxies = settings.KnownProxies;
        }
    }
}
