using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Admin;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest s_manifest;

    static ResourceManagementOptionsConfiguration()
    {
        s_manifest = new ResourceManifest();

        s_manifest
            .DefineScript("admin-quick-navigation")
            .SetDependencies("bootstrap:5")
            .SetUrl("~/OrchardCore.Admin/Scripts/quick-navigation/quick-navigation.min.js", "~/OrchardCore.Admin/Scripts/quick-navigation/quick-navigation.js")
            .SetVersion("1.0.0");

        s_manifest
            .DefineStyle("admin-quick-navigation")
            .SetDependencies("font-awesome:7")
            .SetUrl("~/OrchardCore.Admin/Styles/quick-navigation.min.css", "~/OrchardCore.Admin/Styles/quick-navigation.css")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(s_manifest);
    }
}
