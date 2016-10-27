using Orchard.ResourceManagement;

namespace Orchard.Modules
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest
                .DefineScript("features")
                .SetDependencies("jQuery")
                .SetUrl("/Orchard.Modules/Scripts/features.min.js", "/Orchard.Modules/Scripts/features.js");

            manifest
                .DefineStyle("modules")
                .SetUrl("/Orchard.Modules/Styles/modules.min.css", "/Orchard.Modules/Styles/modules.css");
        }
    }
}
