using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheComingSoonTheme
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static readonly ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("TheComingSoonTheme-bootstrap-bundle")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p", "sha384-8fq7CZc5BnER+jVlJI2Jafpbn4A9320TKhNJfYP33nneHep7sUg/OD30x7fK09pS")
                .SetVersion("5.1.3");

            _manifest
                .DefineScript("coming-soon")
                .SetUrl("~/TheComingSoonTheme/js/scripts.min.js", "TheComingSoonTheme/js/scripts.js")
                .SetVersion("6.0.5");

            _manifest
                .DefineStyle("coming-soon")
                .SetUrl("~/TheComingSoonTheme/css/styles.min.css", "TheComingSoonTheme/css/styles.css")
                .SetVersion("6.0.5");

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
