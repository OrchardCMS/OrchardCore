using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes;

public sealed class ResourceManagementOptionsConfiguration
    : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest s_manifest;

    static ResourceManagementOptionsConfiguration()
    {
        s_manifest = new ResourceManifest();

        s_manifest
            .DefineScript("theme-head")
            .SetUrl(
                "~/OrchardCore.Themes/Scripts/theme-head/theme-head.min.js",
                "~/OrchardCore.Themes/Scripts/theme-head/theme-head.js"
            )
            .SetVersion("1.0.0");

        s_manifest
            .DefineScript("theme-manager")
            .SetUrl(
                "~/OrchardCore.Themes/Scripts/theme-manager/theme-manager.min.js",
                "~/OrchardCore.Themes/Scripts/theme-manager/theme-manager.js"
            )
            .SetDependencies("theme-head")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(s_manifest);
    }
}
