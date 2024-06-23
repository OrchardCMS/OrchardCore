using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.ContentFields;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("trumbowyg-theme")
            .SetUrl("~/OrchardCore.ContentFields/Scripts/trumbowyg.theme.min.js", "~/OrchardCore.ContentFields/Scripts/trumbowyg.theme.js")
            .SetDependencies("trumbowyg", "admin-head")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript("trumbowyg-media-url")
            .SetUrl("~/OrchardCore.ContentFields/Scripts/trumbowyg.media.url.min.js", "~/OrchardCore.ContentFields/Scripts/trumbowyg.media.url.js")
            .SetDependencies("trumbowyg")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript("trumbowyg-media-tag")
            .SetUrl("~/OrchardCore.ContentFields/Scripts/trumbowyg.media.tag.min.js", "~/OrchardCore.ContentFields/Scripts/trumbowyg.media.tag.js")
            .SetDependencies("trumbowyg")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
