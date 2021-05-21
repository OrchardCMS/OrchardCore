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
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.0.1/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.0.1/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-gtEjrD/SeCtmISkJkNUaaKMoLD0//ElJ19smozuHV6z3Iehds+3Ulb9Bn9Plx0x4", "sha384-zlQmapo6noJSGz1A/oxylOFtN0k8EiXX45sOWv3x9f/RGYG0ECMxTbMao6+OLt2e")
                .SetVersion("5.0.1");

            _manifest
                .DefineStyle("TheBlogTheme-bootstrap-oc")
                .SetUrl("~/TheBlogTheme/css/bootstrap-oc.min.css", "~/TheBlogTheme/css/bootstrap-oc.css")
                .SetVersion("1.0.0");

            _manifest
                .DefineScript("TheBlogTheme")
                .SetUrl("~/TheBlogTheme/js/scripts.min.js", "~/TheBlogTheme/js/scripts.js")
                .SetVersion("6.0.0");

            _manifest
                .DefineStyle("TheBlogTheme")
                .SetUrl("~/TheBlogTheme/css/styles.min.css", "~/TheBlogTheme/css/styles.css")
                .SetVersion("6.0.0");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
