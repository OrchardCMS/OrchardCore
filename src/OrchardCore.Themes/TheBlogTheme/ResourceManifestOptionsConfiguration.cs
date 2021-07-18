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
                .SetCdn("https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.min.js", "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.js")
                .SetCdnIntegrity("sha384-MrcW6ZMFYlzcLA8Nl+NtUVF0sA7MsXsP1UyJoMp4YLEuNSfAP+JcXn/tWtIaxVXM", "sha384-5nxO28basTN0oojjESwP8Qm4oVsGeeMYOqXZTlbkw/fIOmXQcJnrDX3O6HBAmdDz")
                .SetVersion("5.0.2");

            _manifest
                .DefineStyle("TheBlogTheme-bootstrap-oc")
                .SetUrl("~/TheBlogTheme/css/bootstrap-oc.min.css", "~/TheBlogTheme/css/bootstrap-oc.css")
                .SetVersion("1.0.0");

            _manifest
                .DefineScript("TheBlogTheme")
                .SetDependencies("TheBlogTheme-bootstrap-bundle")
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
