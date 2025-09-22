using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace OrchardCore.ReverseProxy.Services;

public sealed class ForwardedHeadersOptionsConfiguration : IConfigureOptions<ForwardedHeadersOptions>
{
    private readonly ReverseProxyService _reverseProxyService;

    public ForwardedHeadersOptionsConfiguration(ReverseProxyService reverseProxyService)
    {
        _reverseProxyService = reverseProxyService;
    }

    public void Configure(ForwardedHeadersOptions options)
    {
        var reverseProxySettings = _reverseProxyService.GetSettingsAsync().GetAwaiter().GetResult();
        options.ForwardedHeaders = reverseProxySettings.ForwardedHeaders;

        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();

        foreach (var network in reverseProxySettings.KnownNetworks)
        {
            options.KnownNetworks.Add(IPNetwork.Parse(network));
        }

        foreach (var proxy in reverseProxySettings.KnownProxies)
        {
            options.KnownProxies.Add(System.Net.IPAddress.Parse(proxy));
        }
    }
}
