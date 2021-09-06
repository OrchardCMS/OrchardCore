using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheAdmin
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            // See https://github.com/marella/material-icons for usage
            _manifest
                .DefineStyle("material-icons")
                .SetCdn("https://fonts.googleapis.com/icon?family=Material+Icons|Material+Icons+Outlined|Material+Icons+Two+Tone|Material+Icons+Round|Material+Icons+Sharp")
                .SetUrl("~/TheAdmin/fonts/material-icons/material-icons.min.css", "~/TheAdmin/fonts/material-icons/material-icons.css")
                .SetVersion("0.7.2");

            _manifest
                .DefineScript("admin")
                .SetDependencies("jQuery")
                .SetUrl("~/TheAdmin/js/TheAdmin.min.js", "~/TheAdmin/js/TheAdmin.js")
                .SetVersion("1.0.0");

            _manifest
                .DefineScript("admin-head")
                .SetUrl("~/TheAdmin/js/TheAdmin-header.min.js", "~/TheAdmin/js/TheAdmin-header.js")
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
