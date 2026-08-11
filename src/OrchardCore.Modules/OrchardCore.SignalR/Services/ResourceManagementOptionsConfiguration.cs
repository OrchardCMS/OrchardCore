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
                "sha384-LFXLuRjjsQwcv952UfdojlO49RKdAHfWeroWCC5f0IATQOWb34SAen4oPd3BG0lD",
                "sha384-lupBX7CgtSYWkwMikCeokm2xtDKKf6w0/GRawcVk19pZc7WzY9Dl3cg8Cp0uC4e1")
            .SetVersion("10.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
