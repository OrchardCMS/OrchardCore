using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheAgencyTheme
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("TheAgencyTheme-bootstrap-bundle")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.1.1/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.1.1/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-/bQdsTh/da6pkI1MST/rWKFNjaCP5gBSY4sEBT38Q/9RBh9AH40zEOg7Hlq2THRZ", "sha384-5tkUMcue1IzwOrYYLqA5A9bSIk8vgFPWvPRRic7R+jUyDtqOVdrhzpTS9C4ThRYV")
                .SetVersion("5.1.1");

            _manifest
                .DefineScript("TheAgencyTheme")
                .SetUrl("~/TheAgencyTheme/js/scripts.min.js", "~/TheAgencyTheme/js/scripts.js")
                .SetVersion("7.0.7");

            _manifest
                .DefineStyle("TheAgencyTheme")
                .SetUrl("~/TheAgencyTheme/css/styles.min.css", "~/TheAgencyTheme/css/styles.css")
                .SetVersion("7.0.7");

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
}
