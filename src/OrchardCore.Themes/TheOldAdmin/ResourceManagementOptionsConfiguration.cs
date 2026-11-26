using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheOldAdmin;

public sealed class ResourceManagementOptionsConfiguration
    : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest s_manifest;

    static ResourceManagementOptionsConfiguration()
    {
        s_manifest = new ResourceManifest();

        s_manifest
            .DefineScript("the-old-admin")
            .SetDependencies("bootstrap", "the-old-admin-main", "theme-manager", "Sortable")
            .SetUrl("~/TheOldAdmin/js/theoldadmin/TheOldAdmin.min.js", "~/TheOldAdmin/js/theoldadmin/TheOldAdmin.js")
            .SetVersion("1.0.0");

        s_manifest
            .DefineScript("the-old-admin-main")
            .SetUrl(
                "~/TheOldAdmin/js/theoldadmin-main/TheOldAdmin-main.min.js",
                "~/TheOldAdmin/js/theoldadmin-main/TheOldAdmin-main.js"
            )
            .SetDependencies("bootstrap", "theme-head", "js-cookie")
            .SetVersion("1.0.0");

        s_manifest
            .DefineStyle("the-old-admin")
            .SetUrl("~/TheOldAdmin/css/TheOldAdmin.min.css", "~/TheOldAdmin/css/TheOldAdmin.css")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(s_manifest);
    }
}
