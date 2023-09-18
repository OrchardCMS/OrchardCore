using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheAdmin
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static readonly ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("admin")
                .SetDependencies("bootstrap", "jQuery")
                .SetUrl("~/TheAdmin/js/TheAdmin.min.js", "~/TheAdmin/js/TheAdmin.js")
                .SetVersion("1.0.0");

            _manifest
                .DefineScript("admin-head")
                .SetUrl("~/TheAdmin/js/TheAdmin-header.min.js", "~/TheAdmin/js/TheAdmin-header.js")
                .SetDependencies("js-cookie")
                .SetVersion("1.0.0");

            _manifest
                .DefineStyle("admin")
                .SetUrl("~/TheAdmin/css/TheAdmin.min.css", "~/TheAdmin/css/TheAdmin.css")
                .SetVersion("1.0.0");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
