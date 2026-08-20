using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Media;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest s_manifest;

    static ResourceManagementOptionsConfiguration()
    {
        s_manifest = new ResourceManifest();

        s_manifest
            .DefineScript("media")
            .SetUrl("~/OrchardCore.Media/Scripts/media2.min.js", "~/OrchardCore.Media/Scripts/media2.js")
            .SetVersion("2.0.0")
            .SetAttribute("type", "module");

        s_manifest
            .DefineStyle("media")
            .SetUrl("~/OrchardCore.Media/Styles/media2.min.css", "~/OrchardCore.Media/Styles/media2.css")
            .SetVersion("2.0.0");

        s_manifest
            .DefineScript("media-picker")
            .SetUrl("~/OrchardCore.Media/Scripts/media-picker2.min.js", "~/OrchardCore.Media/Scripts/media-picker2.js")
            .SetVersion("2.0.0")
            .SetAttribute("type", "module");

        s_manifest
            .DefineStyle("media-picker")
            .SetUrl("~/OrchardCore.Media/Styles/media-picker2.min.css", "~/OrchardCore.Media/Styles/media-picker2.css")
            .SetVersion("2.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(s_manifest);
    }
}
