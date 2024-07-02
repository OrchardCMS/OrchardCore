using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("theme-head")
            .SetUrl("~/OrchardCore.Themes/Scripts/theme-head.min.js", "~/OrchardCore.Themes/Scripts/theme-head.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript("theme-manager")
            .SetUrl("~/OrchardCore.Themes/Scripts/theme-manager.min.js", "~/OrchardCore.Themes/Scripts/theme-manager.js")
            .SetDependencies("theme-head")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
