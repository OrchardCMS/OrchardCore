using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Spatial
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("leaflet")
                .SetUrl("/OrchardCore.Spatial/Scripts/leaflet/leaflet.js", "/OrchardCore.Spatial/Scripts/leaflet/leaflet-src.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.0/leaflet.min.js", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.0/leaflet-src.js")
                .SetCdnIntegrity("sha384-yQhbJ10eE43LMul1CMgyQTRswwLBH94MQMO/Wq296rlHPUET+rznlxfQPtupouoX", "sha384-OpEbGD+GgOvGzqHl/VuP+X8cGvdUoPCw+F31C8ddag7seV0wOtUIChX0wCAoyZVD")
                .SetVersion("1.9.0");

            _manifest
                .DefineStyle("leaflet")
                .SetUrl("/OrchardCore.Spatial/Styles/leaflet.min.css", "/OrchardCore.Spatial/Styles/leaflet.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.0/leaflet.min.css", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.0/leaflet.css")
                .SetCdnIntegrity("sha384-oPNdwszLJvgXV8sIi6zYKJBYI/ST8rVoT8MyjqxOZ+W/nStKywI3wKguIB8D95pi", "sha384-kxXhFDZB0L84bBV/apPOb8zGC+fsQ1dBPpKXPUXc1zRymi4BaueVyC27iDDPdssp")
                .SetVersion("1.9.0");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
