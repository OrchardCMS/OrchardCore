using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.Settings;
using OrchardCore.Settings.Services;

namespace OrchardCore.ReverseProxy.Services;

public sealed class ForwardedHeadersOptionsConfiguration : IConfigureOptions<ForwardedHeadersOptions>
{
    private readonly ReverseProxyService _reverseProxyService;
    private readonly ConfigurableSettingsServiceFactory<ReverseProxySettings> _settingsFactory;

    public ForwardedHeadersOptionsConfiguration(
        ReverseProxyService reverseProxyService,
        ConfigurableSettingsServiceFactory<ReverseProxySettings> settingsFactory)
    {
        _reverseProxyService = reverseProxyService;
        _settingsFactory = settingsFactory;
    }

    public void Configure(ForwardedHeadersOptions options)
    {
        // Get database settings via the existing service
        var databaseSettings = _reverseProxyService.GetSettingsAsync().GetAwaiter().GetResult();

        // Merge with file configuration using the factory (singleton-safe)
        var reverseProxySettings = _settingsFactory.MergeSettings(databaseSettings);

        options.ForwardedHeaders = reverseProxySettings.ForwardedHeaders;

        // In .NET 10, KnownNetworks is obsolete, use KnownIPNetworks instead
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();

        foreach (var network in reverseProxySettings.KnownNetworks ?? [])
        {
            if (!string.IsNullOrWhiteSpace(network))
            {
                // In .NET 10, use System.Net.IPNetwork instead of the obsolete IPNetwork
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
            }
        }

        foreach (var proxy in reverseProxySettings.KnownProxies ?? [])
        {
            if (!string.IsNullOrWhiteSpace(proxy))
            {
                options.KnownProxies.Add(System.Net.IPAddress.Parse(proxy));
            }
        }
    }
}
