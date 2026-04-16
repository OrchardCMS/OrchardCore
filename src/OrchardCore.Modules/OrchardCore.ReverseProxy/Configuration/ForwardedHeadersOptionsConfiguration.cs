using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.Settings;

namespace OrchardCore.ReverseProxy.Configuration;

internal sealed class ForwardedHeadersOptionsConfiguration : IConfigureOptions<ForwardedHeadersOptions>
{
    private readonly ISiteService _siteService;

    public ForwardedHeadersOptionsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(ForwardedHeadersOptions options)
    {
        var settings = _siteService.GetSettings<ReverseProxySettings>();

        options.ForwardedHeaders = settings.ForwardedHeaders;

        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();

        foreach (var network in settings.KnownNetworks)
        {
            options.KnownIPNetworks.Add(IPNetwork.Parse(network));
        }

        foreach (var proxy in settings.KnownProxies)
        {
            options.KnownProxies.Add(IPAddress.Parse(proxy));
        }
    }
}
