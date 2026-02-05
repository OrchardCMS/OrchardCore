using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.ReverseProxy;
using OrchardCore.ReverseProxy.Settings;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Configures ReverseProxy settings from appsettings.json using the configurable settings infrastructure.
    /// </summary>
    /// <param name="builder">The Orchard Core builder.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// This enables configuration of ReverseProxy settings through appsettings.json:
    /// <code>
    /// {
    ///   "OrchardCore_ReverseProxy": {
    ///     "ForwardedHeaders": "XForwardedFor, XForwardedProto",
    ///     "KnownNetworks": ["10.0.0.0/8"],
    ///     "KnownProxies": ["192.168.1.1"],
    ///     "DisableUIConfiguration": false
    ///   }
    /// }
    /// </code>
    /// </remarks>
    public static OrchardCoreBuilder ConfigureReverseProxySettings(this OrchardCoreBuilder builder)
    {
        return builder.ConfigureSettings<ReverseProxySettings>(Startup.ConfigurationKey);
    }
}
