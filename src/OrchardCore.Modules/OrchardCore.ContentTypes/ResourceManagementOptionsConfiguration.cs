using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.ContentTypes;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("list-items-filter")
            .SetUrl("~/OrchardCore.ContentTypes/Scripts/list-items-filter.min.js", "~/OrchardCore.ContentTypes/Scripts/list-items-filter.js")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
