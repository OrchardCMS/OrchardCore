using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.ContentFields;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    public void Configure(ResourceManagementOptions options)
    {
        var manifest = new ResourceManifest();

        manifest
            .DefineScript("trumbowyg-media-url")
            .SetUrl("~/OrchardCore.ContentFields/Scripts/trumbowyg.media.url.min.js", "~/OrchardCore.ContentFields/Scripts/trumbowyg.media.url.js")
            .SetDependencies("trumbowyg")
            .SetVersion("1.0.0");

        manifest
            .DefineScript("trumbowyg-media-tag")
            .SetUrl("~/OrchardCore.ContentFields/Scripts/trumbowyg.media.tag.min.js", "~/OrchardCore.ContentFields/Scripts/trumbowyg.media.tag.js")
            .SetDependencies("trumbowyg")
            .SetVersion("1.0.0");

        options.ResourceManifests.Add(manifest);
    }
}
