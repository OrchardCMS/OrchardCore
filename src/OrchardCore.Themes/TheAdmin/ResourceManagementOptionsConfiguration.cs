using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheAdmin;

public sealed class ResourceManagementOptionsConfiguration
    : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest s_manifest;

    static ResourceManagementOptionsConfiguration()
    {
        s_manifest = new ResourceManifest();

        s_manifest
            .DefineScript("the-admin")
            .SetDependencies("bootstrap", "admin-main", "theme-manager", "Sortable")
            .SetUrl("~/TheAdmin/js/theadmin/TheAdmin.min.js", "~/TheAdmin/js/theadmin/TheAdmin.js")
            .SetVersion("1.0.0");

        s_manifest
            .DefineScript("admin-main")
            .SetUrl(
                "~/TheAdmin/js/theadmin-main/TheAdmin-main.min.js",
                "~/TheAdmin/js/theadmin-main/TheAdmin-main.js"
            )
            .SetDependencies("bootstrap", "theme-head", "js-cookie")
            .SetVersion("1.0.0");

        s_manifest
            .DefineStyle("the-admin")
            .SetUrl("~/TheAdmin/css/TheAdmin.min.css", "~/TheAdmin/css/TheAdmin.css")
            .SetVersion("1.0.0");

        // The scheme picker of the top bar.
        s_manifest
            .DefineScript("admin-scheme-picker")
            .SetUrl("~/TheAdmin/js/admin-scheme-picker.min.js", "~/TheAdmin/js/admin-scheme-picker.js")
            .SetVersion("1.0.0");

        // The editor of a scheme, with its live preview.
        s_manifest
            .DefineScript("admin-scheme-editor")
            .SetUrl("~/TheAdmin/js/admin-scheme-editor.min.js", "~/TheAdmin/js/admin-scheme-editor.js")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(s_manifest);
    }
}
