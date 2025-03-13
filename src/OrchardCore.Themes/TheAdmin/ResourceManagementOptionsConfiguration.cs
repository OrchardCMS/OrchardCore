using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheAdmin;

public sealed class ResourceManagementOptionsConfiguration
    : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("admin")
            .SetDependencies("admin-main", "theme-manager", "jQuery", "Sortable")
            .SetUrl("~/TheAdmin/js/theadmin/TheAdmin.min.js", "~/TheAdmin/js/theadmin/TheAdmin.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript("admin-main")
            .SetUrl(
                "~/TheAdmin/js/theadmin-main/TheAdmin-main.min.js",
                "~/TheAdmin/js/theadmin-main/TheAdmin-main.js"
            )
            .SetDependencies("theme-head", "js-cookie")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle("admin")
            .SetUrl("~/TheAdmin/css/TheAdmin.min.css", "~/TheAdmin/css/TheAdmin.css")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
