using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using OrchardCore.ReverseProxy.Settings;

namespace OrchardCore.ReverseProxy.Configuration;

internal sealed class ForwardedHeadersOptionsConfiguration : IConfigureOptions<ForwardedHeadersOptions>
{
    private readonly IOptionsMonitor<ReverseProxySettings> _reverseProxySettings;

    public ForwardedHeadersOptionsConfiguration(IOptionsMonitor<ReverseProxySettings> reverseProxySettings)
    {
        _reverseProxySettings = reverseProxySettings;
    }

    public void Configure(ForwardedHeadersOptions options)
    {
        // Read the current settings, as this instance is kept by the options monitor.
        var reverseProxySettings = _reverseProxySettings.CurrentValue;

        options.ForwardedHeaders = reverseProxySettings.ForwardedHeaders;

        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();

        foreach (var network in reverseProxySettings.KnownNetworks)
        {
            options.KnownIPNetworks.Add(IPNetwork.Parse(network));
        }

        foreach (var proxy in reverseProxySettings.KnownProxies)
        {
            options.KnownProxies.Add(IPAddress.Parse(proxy));
        }
    }
}
