using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheComingSoonTheme
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("TheComingSoonTheme-bootstrap-bundle")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.0.1/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.0.1/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-gtEjrD/SeCtmISkJkNUaaKMoLD0//ElJ19smozuHV6z3Iehds+3Ulb9Bn9Plx0x4", "sha384-zlQmapo6noJSGz1A/oxylOFtN0k8EiXX45sOWv3x9f/RGYG0ECMxTbMao6+OLt2e")
                .SetVersion("5.0.1");

            _manifest
                .DefineScript("coming-soon")
                .SetUrl("~/TheComingSoonTheme/js/scripts.min.js", "TheComingSoonTheme/js/scripts.js")
                .SetVersion("6.0.0");

            _manifest
                .DefineStyle("coming-soon")
                .SetUrl("~/TheComingSoonTheme/css/styles.min.css", "TheComingSoonTheme/css/styles.css")
                .SetVersion("6.0.0");

            _manifest
                .DefineStyle("TheComingSoonTheme-bootstrap-oc")
                .SetUrl("~/TheComingSoonTheme/css/bootstrap-oc.min.css", "~/TheComingSoonTheme/css/bootstrap-oc.css")
                .SetVersion("1.0.0");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
