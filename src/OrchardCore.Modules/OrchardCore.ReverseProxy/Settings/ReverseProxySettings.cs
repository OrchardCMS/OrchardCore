using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;

namespace OrchardCore.ReverseProxy.Settings;

public class ReverseProxySettings
{
    public ForwardedHeaders ForwardedHeaders { get; set; }

    public string[] KnownNetworks { get; set; } = [];

    public string[] KnownProxies { get; set; } = [];

    [JsonIgnore]
    internal bool FromConfiguration { get; set; }
}
