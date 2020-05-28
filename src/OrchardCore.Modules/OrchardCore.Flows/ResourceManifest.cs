using OrchardCore.ResourceManagement;

namespace OrchardCore.Flows
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("oc-flows-edit")
                .SetUrl("~/OrchardCore.Flows/Scripts/flows.edit.min.js",
                        "~/OrchardCore.Flows/Scripts/flows.edit.js")
                .SetDependencies("oc-widgets-edit")
                .SetVersion("1.0");

            manifest
                .DefineStyle("oc-flows-edit")
                .SetDependencies("oc-widgets-edit")
                .SetUrl("~/OrchardCore.Flows/Styles/flows.edit.min.css",
                        "~/OrchardCore.Flows/Styles/flows.edit.css")
                .SetVersion("1.0");
        }
    }
}
