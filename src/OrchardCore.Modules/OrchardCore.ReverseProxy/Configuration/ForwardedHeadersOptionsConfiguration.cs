using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using OrchardCore.ReverseProxy.Settings;

namespace OrchardCore.ReverseProxy.Configuration;

internal sealed class ForwardedHeadersOptionsConfiguration : IConfigureOptions<ForwardedHeadersOptions>
{
    private readonly ReverseProxySettings _reverseProxySettings;

    public ForwardedHeadersOptionsConfiguration(IOptions<ReverseProxySettings> reverseProxySettingsOptions)
    {
        _reverseProxySettings = reverseProxySettingsOptions.Value;
    }

    public void Configure(ForwardedHeadersOptions options)
    {
        options.ForwardedHeaders = _reverseProxySettings.ForwardedHeaders;

        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();

        foreach (var network in _reverseProxySettings.KnownNetworks)
        {
            options.KnownIPNetworks.Add(IPNetwork.Parse(network));
        }

        foreach (var proxy in _reverseProxySettings.KnownProxies)
        {
            options.KnownProxies.Add(IPAddress.Parse(proxy));
        }
    }
}
