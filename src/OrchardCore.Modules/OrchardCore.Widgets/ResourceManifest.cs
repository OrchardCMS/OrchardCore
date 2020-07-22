using OrchardCore.ResourceManagement;

namespace OrchardCore.Widgets
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("oc-widgets-edit")
                .SetUrl("~/OrchardCore.Widgets/Scripts/widgetslist.edit.min.js",
                        "~/OrchardCore.Widgets/Scripts/widgetslist.edit.js")
                .SetDependencies("jquery-ui", "admin")
                .SetVersion("1.0");

            manifest
                .DefineStyle("oc-widgets-edit")
                .SetDependencies("bootstrap")
                .SetUrl("~/OrchardCore.Widgets/Styles/widgetslist.edit.min.css",
                        "~/OrchardCore.Widgets/Styles/widgetslist.edit.css")
                .SetVersion("1.0");
        }
    }
}
