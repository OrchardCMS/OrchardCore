using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.SignalR.Services;

internal sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest.DefineScript("signalr")
            .SetUrl(
                "~/OrchardCore.SignalR/Scripts/signalr.min.js",
                "~/OrchardCore.SignalR/Scripts/signalr.js")
            .SetCdn(
                "https://cdn.jsdelivr.net/npm/@microsoft/signalr@10.0.0/dist/browser/signalr.min.js",
                "https://cdn.jsdelivr.net/npm/@microsoft/signalr@10.0.0/dist/browser/signalr.js")
            .SetCdnIntegrity(
                "sha384-WlPNGnp2GG/eMuGnbuG2p5ov9pzHsqoPQbzjOz61V608d7JfDa+3abt0YOoyUwU5",
                "sha384-MmGm086ejVZQz6uje3dR3Rf3r9/LCN/xxlYU35mzIg/yqtAuDKLzrd6zU1nQmdDT")
            .SetVersion("10.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
