using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Facebook
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static readonly ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("fb")
                .SetDependencies("fbsdk")
                .SetUrl("~/OrchardCore.Facebook/sdk/fb.js");

            _manifest
                .DefineScript("fbsdk")
                .SetUrl("~/OrchardCore.Facebook/sdk/fbsdk.js");
        }

        public void Configure(ResourceManagementOptions options) => options.ResourceManifests.Add(_manifest);
    }
}
