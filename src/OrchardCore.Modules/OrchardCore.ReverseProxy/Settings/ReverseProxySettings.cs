using Microsoft.AspNetCore.HttpOverrides;
using OrchardCore.Settings;

namespace OrchardCore.ReverseProxy.Settings;

public class ReverseProxySettings : IConfigurableSettings
{
    [ConfigurationProperty(
        MergeStrategy = PropertyMergeStrategy.FileOverridesDatabase,
        DisplayName = "Forwarded Headers",
        Description = "The HTTP headers to forward from the reverse proxy.")]
    public ForwardedHeaders ForwardedHeaders { get; set; }

    [ConfigurationProperty(
        MergeStrategy = PropertyMergeStrategy.Merge,
        DisplayName = "Known Networks",
        Description = "IP networks that are known to contain reverse proxies (CIDR notation).")]
    public string[] KnownNetworks { get; set; } = [];

    [ConfigurationProperty(
        MergeStrategy = PropertyMergeStrategy.Merge,
        DisplayName = "Known Proxies",
        Description = "IP addresses of known reverse proxies.")]
    public string[] KnownProxies { get; set; } = [];

    /// <inheritdoc/>
    public bool DisableUIConfiguration { get; set; }
}
