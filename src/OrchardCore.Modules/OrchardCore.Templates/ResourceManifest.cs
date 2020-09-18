using OrchardCore.ResourceManagement;

namespace OrchardCore.Templates
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("templatepreview-edit")
                .SetUrl("~/OrchardCore.Templates/Scripts/templatepreview.edit.min.js", "~/OrchardCore.Templates/Scripts/templatepreview.edit.js")
                .SetVersion("1.0.0");
        }
    }
}
