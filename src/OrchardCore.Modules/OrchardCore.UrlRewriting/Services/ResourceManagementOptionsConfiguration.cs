using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.UrlRewriting.Services;

internal sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("sortable-rules")
            .SetDependencies("Sortable")
            .SetUrl("~/OrchardCore.UrlRewriting/Scripts/sortable-rules.min.js", "~/OrchardCore.UrlRewriting/Scripts/sortable-rules.js");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
