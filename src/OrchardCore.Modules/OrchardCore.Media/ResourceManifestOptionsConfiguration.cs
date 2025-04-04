using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Media;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("media")
            .SetUrl("~/OrchardCore.Media/Scripts/media.min.js", "~/OrchardCore.Media/Scripts/media.js")
            .SetDependencies("vuejs:2", "Sortable", "vue-draggable:2", "jQuery-ui", "credential-helpers", "bootstrap")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript("media")
            .SetUrl(
                "~/OrchardCore.Media/Scripts/media.min.js",
                "~/OrchardCore.Media/Scripts/media.js"
            )
            .SetDependencies("vuejs:2", "sortable", "vuedraggable:2", "jQuery-ui")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle("media")
            .SetUrl(
                "~/OrchardCore.Media/Styles/media.min.css",
                "~/OrchardCore.Media/Styles/media.css"
            )
            .SetVersion("1.0.0");

        _manifest
            .DefineScript("media")
            .SetUrl("~/OrchardCore.Media/Scripts/media2.js")
            .SetDependencies("vuejs:3", "sortable", "vuedraggable:3", "jQuery-iframe-transport")
            .SetVersion("2.0.0")
            .SetAttribute("type", "module");

        _manifest
            .DefineStyle("media")
            .SetUrl("~/OrchardCore.Media/Styles/media2.css")
            .SetVersion("2.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
