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
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.1.0/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.1.0/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-U1DAWAznBHeqEIlVSCgzq+c9gqGAJn5c/t99JyeKa9xxaYpSvHU5awsuZVVFIhvj", "sha384-sCrrXXsCVYsmCuGTFZDBWJBhcTU5N2csSa8rhGERa1/tCRBHcJEcxG3ivcPvx3t6")
                .SetVersion("5.1.0");

            _manifest
                .DefineScript("coming-soon")
                .SetUrl("~/TheComingSoonTheme/js/scripts.min.js", "TheComingSoonTheme/js/scripts.js")
                .SetVersion("6.0.4");

            _manifest
                .DefineStyle("coming-soon")
                .SetUrl("~/TheComingSoonTheme/css/styles.min.css", "TheComingSoonTheme/css/styles.css")
                .SetVersion("6.0.4");

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
