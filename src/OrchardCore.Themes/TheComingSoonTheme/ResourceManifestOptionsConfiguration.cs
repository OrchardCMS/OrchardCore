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
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-MrcW6ZMFYlzcLA8Nl+NtUVF0sA7MsXsP1UyJoMp4YLEuNSfAP+JcXn/tWtIaxVXM", "sha384-5nxO28basTN0oojjESwP8Qm4oVsGeeMYOqXZTlbkw/fIOmXQcJnrDX3O6HBAmdDz")
                .SetVersion("5.0.2");

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
