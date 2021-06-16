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

            _manifest
                .DefineStyle("material-icons")
                .SetCdn("https://fonts.googleapis.com/icon?family=Material+Icons|Material+Icons+Outlined|Material+Icons+Two+Tone|Material+Icons+Round|Material+Icons+Sharp")
                .SetUrl("~/TheAdmin/fonts/material-design-icons/material-icons.css")
                .SetVersion("3.0.1");

            _manifest
                .DefineScript("the-admin")
                .SetUrl("~/TheAdmin/js/TheAdmin.min.js", "~/TheAdmin/js/TheAdmin.js")
                .SetVersion("1.0.0");

            _manifest
                .DefineScript("the-admin-head")
                .SetUrl("~/TheAdmin/js/TheAdmin-header.min.js", "~/TheAdmin/js/TheAdmin-header.js")
                .SetVersion("1.0.0");

            _manifest
                .DefineStyle("the-admin")
                .SetUrl("~/TheAdmin/css/TheAdmin.min.css", "~/TheAdmin/css/TheAdmin.css")
                .SetVersion("1.0.0");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
