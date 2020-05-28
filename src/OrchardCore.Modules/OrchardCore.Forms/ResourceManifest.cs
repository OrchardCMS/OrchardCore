using OrchardCore.ResourceManagement;

namespace OrchardCore.Forms
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineStyle("oc-forms-edit")
                .SetDependencies("oc-flows-edit")
                .SetUrl("~/OrchardCore.Forms/Styles/forms.edit.min.css",
                        "~/OrchardCore.Forms/Styles/forms.edit.css")
                .SetVersion("1.0");
        }
    }
}
