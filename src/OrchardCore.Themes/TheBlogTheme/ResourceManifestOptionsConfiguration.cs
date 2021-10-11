using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheBlogTheme
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("TheBlogTheme-bootstrap-bundle")
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.1.1/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.1.1/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-/bQdsTh/da6pkI1MST/rWKFNjaCP5gBSY4sEBT38Q/9RBh9AH40zEOg7Hlq2THRZ", "sha384-5tkUMcue1IzwOrYYLqA5A9bSIk8vgFPWvPRRic7R+jUyDtqOVdrhzpTS9C4ThRYV")
                .SetVersion("5.1.1");

            _manifest
                .DefineStyle("TheBlogTheme-bootstrap-oc")
                .SetUrl("~/TheBlogTheme/css/bootstrap-oc.min.css", "~/TheBlogTheme/css/bootstrap-oc.css")
                .SetVersion("1.0.0");

            _manifest
                .DefineScript("TheBlogTheme")
                .SetDependencies("TheBlogTheme-bootstrap-bundle")
                .SetUrl("~/TheBlogTheme/js/scripts.min.js", "~/TheBlogTheme/js/scripts.js")
                .SetVersion("6.0.6");

            _manifest
                .DefineStyle("TheBlogTheme")
                .SetUrl("~/TheBlogTheme/css/styles.min.css", "~/TheBlogTheme/css/styles.css")
                .SetVersion("6.0.6");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
