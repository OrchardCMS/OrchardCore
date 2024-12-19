using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.ContentPreview;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("contentpreview-edit")
            .SetUrl("~/OrchardCore.ContentPreview/Scripts/contentpreview.edit.min.js", "~/OrchardCore.ContentPreview/Scripts/contentpreview.edit.js")
            .SetDependencies("jQuery")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
