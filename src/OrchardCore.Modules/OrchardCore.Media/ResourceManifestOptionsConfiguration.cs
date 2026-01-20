using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Media
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("media")
                .SetUrl("~/OrchardCore.Media/Scripts/media.min.js", "~/OrchardCore.Media/Scripts/media.js")
                .SetDependencies("vuejs", "sortable", "vuedraggable", "jQuery-ui")
                .SetVersion("1.0.0");

            _manifest
                .DefineStyle("media")
                .SetUrl("~/OrchardCore.Media/Styles/media.min.css", "~/OrchardCore.Media/Styles/media.css")
                .SetVersion("1.0.0");

        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
