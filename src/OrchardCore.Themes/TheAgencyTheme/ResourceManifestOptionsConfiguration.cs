using OrchardCore.ResourceManagement;
using Microsoft.Extensions.Options;

namespace OrchardCore.Themes.TheAgencyTheme
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("TheAgencyTheme")
                .SetDependencies("jQuery")
                .SetUrl("~/TheAgencyTheme/scripts/scripts.min.js", "~/TheAgencyTheme/scripts/scripts.js")
                .SetVersion("6.0.3");

            _manifest
                .DefineStyle("TheAgencyTheme")
                .SetUrl("~/TheAgencyTheme/Styles/styles.min.css", "~/TheAgencyTheme/Styles/styles.css")
                .SetVersion("6.0.3");

            _manifest
                .DefineStyle("TheAgencyTheme-bootstrap-oc")
                .SetDependencies("TheAgencyTheme")
                .SetUrl("~/TheAgencyTheme/Styles/bootstrap-oc.min.css", "~/TheAgencyTheme/Styles/bootstrap-oc.css")
                .SetVersion("1.0.0");
        }

        
        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
