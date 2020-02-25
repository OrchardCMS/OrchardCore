using OrchardCore.ResourceManagement;

namespace OrchardCore.Facebook
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("fb")
                .SetDependencies("fbsdk")
                .SetUrl("~/OrchardCore.Facebook/sdk/fb.js");

            manifest
                .DefineScript("fbsdk")
                .SetUrl("~/OrchardCore.Facebook/sdk/fbsdk.js");
        }
    }
}
