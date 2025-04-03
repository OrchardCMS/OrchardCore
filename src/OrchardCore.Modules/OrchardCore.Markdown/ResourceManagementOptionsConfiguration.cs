using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Markdown;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("easymde")
            .SetUrl("~/OrchardCore.Markdown/Scripts/easymde.min.js")
            .SetVersion("2.18.0");

        _manifest
            .DefineStyle("easymde")
            .SetUrl(
                "~/OrchardCore.Markdown/Styles/mde.min.css",
                "~/OrchardCore.Markdown/Styles/mde.css"
            )
            .SetVersion("2.18.0");

        _manifest
            .DefineScript("easymde-mediatoolbar")
            .SetDependencies("easymde")
            .SetUrl(
                "~/OrchardCore.Markdown/Scripts/mde.mediatoolbar.min.js",
                "~/OrchardCore.Markdown/Scripts/mde.mediatoolbar.js"
            )
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
