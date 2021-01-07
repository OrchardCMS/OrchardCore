using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheAgencyTheme
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("TheAgencyTheme")
                .SetDependencies("jQuery")
                .SetUrl("~/TheAgencyTheme/scripts/scripts.min.css", "~/TheAgencyTheme/scripts/scripts.js")
                .SetVersion("6.0.3");

            manifest
                .DefineStyle("TheAgencyTheme")
                .SetUrl("~/TheAgencyTheme/Styles/styles.min.css", "~/TheAgencyTheme/Styles/styles.css")
                .SetVersion("6.0.3");

            manifest
                .DefineStyle("TheAgencyTheme-bootstrap-oc")
                .SetDependencies("TheAgencyTheme")
                .SetUrl("~/TheAgencyTheme/Styles/bootstrap-oc.min.css", "~/TheAgencyTheme/Styles/bootstrap-oc.css")
                .SetVersion("1.0.0");
        }
    }
}
