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
            .SetUrl("~/OrchardCore.Media/Scripts/media2.min.js", "~/OrchardCore.Media/Scripts/media2.js")
            .SetVersion("2.0.0")
            .SetAttribute("type", "module");

        _manifest
            .DefineStyle("media")
            .SetUrl("~/OrchardCore.Media/Styles/media2.min.css", "~/OrchardCore.Media/Styles/media2.css")
            .SetVersion("2.0.0");

        _manifest
            .DefineScript("media-picker")
            .SetUrl("~/OrchardCore.Media/Scripts/media-picker2.min.js", "~/OrchardCore.Media/Scripts/media-picker2.js")
            .SetVersion("2.0.0")
            .SetAttribute("type", "module");

        _manifest
            .DefineStyle("media-picker")
            .SetUrl("~/OrchardCore.Media/Styles/media-picker2.min.css", "~/OrchardCore.Media/Styles/media-picker2.css")
            .SetVersion("2.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
