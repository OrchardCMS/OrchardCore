using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheCommunityTheme;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("TheCommunityTheme-bootstrap-bundle")
            .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.js")
            .SetCdnIntegrity("sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz", "sha384-ga0NLkToyMqEMFx4qJP3OgoiGlGq6r8EidUXlWBiid22ckMe1MBDkvBHjmY2Svz6")
            .SetVersion("5.3.3");

        _manifest
            .DefineScript("TheCommunityTheme")
            .SetDependencies("TheCommunityTheme-bootstrap-bundle")
            .SetUrl("~/TheCommunityTheme/js/scripts.min.js", "~/TheCommunityTheme/js/scripts.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle("TheCommunityTheme")
            .SetUrl("~/TheCommunityTheme/css/styles.min.css", "~/TheCommunityTheme/css/styles.css")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle("TheCommunityTheme-bootstrap-oc")
            .SetDependencies("TheCommunityTheme")
            .SetUrl("~/TheCommunityTheme/css/bootstrap-oc.min.css", "~/TheCommunityTheme/css/bootstrap-oc.css")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
