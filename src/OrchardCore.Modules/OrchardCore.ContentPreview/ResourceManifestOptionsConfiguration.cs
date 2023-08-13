using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.ContentPreview
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static readonly ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("contentpreview-edit")
                .SetDependencies("admin")
                .SetUrl("~/OrchardCore.ContentPreview/Scripts/contentpreview.edit.min.js", "~/OrchardCore.ContentPreview/Scripts/contentpreview.edit.js")
                .SetVersion("1.0.0");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
