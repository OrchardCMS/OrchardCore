using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheAgencyTheme;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("TheAgencyTheme-bootstrap-bundle")
            .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.3.6/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.3.6/dist/js/bootstrap.bundle.js")
            .SetCdnIntegrity("sha384-j1CDi7MgGQ12Z7Qab0qlWQ/Qqz24Gc6BM0thvEMVjHnfYGF0rmFCozFSxQBxwHKO", "sha384-tcxuNjmU/dcp769u0TRMOD45CdtuScYrGA1/LBcwQA8Oex/WkXW5maQM1hIIbSvF")
            .SetVersion("5.3.6");

        _manifest
            .DefineScript("TheAgencyTheme")
            .SetUrl("~/TheAgencyTheme/js/scripts.min.js", "~/TheAgencyTheme/js/scripts.js")
            .SetVersion("7.0.10");

        _manifest
            .DefineStyle("TheAgencyTheme")
            .SetUrl("~/TheAgencyTheme/css/styles.min.css", "~/TheAgencyTheme/css/styles.css")
            .SetVersion("7.0.10");

        _manifest
            .DefineStyle("TheAgencyTheme-bootstrap-oc")
            .SetDependencies("TheAgencyTheme")
            .SetUrl("~/TheAgencyTheme/css/bootstrap-oc.min.css", "~/TheAgencyTheme/css/bootstrap-oc.css")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
