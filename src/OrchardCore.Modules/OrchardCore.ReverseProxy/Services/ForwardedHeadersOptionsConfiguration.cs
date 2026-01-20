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

        // In .NET 10, KnownNetworks is obsolete, use KnownIPNetworks instead
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();

        foreach (var network in reverseProxySettings.KnownNetworks)
        {
            // In .NET 10, use System.Net.IPNetwork instead of the obsolete IPNetwork
            options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
        }

        foreach (var proxy in reverseProxySettings.KnownProxies)
        {
            options.KnownProxies.Add(System.Net.IPAddress.Parse(proxy));
        }
    }
}
