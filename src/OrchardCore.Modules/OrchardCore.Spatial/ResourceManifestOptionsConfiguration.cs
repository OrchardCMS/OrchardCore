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
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.3.1/leaflet.js", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.3.1/leaflet.js")
                .SetCdnIntegrity("sha256-CNm+7c26DTTCGRQkM9vp7aP85kHFMqs9MhPEuytF+fQ=", "sha256-CNm+7c26DTTCGRQkM9vp7aP85kHFMqs9MhPEuytF+fQ=")
                .SetVersion("1.7.1")
                ;

            _manifest
                .DefineStyle("leaflet")
                .SetUrl("/OrchardCore.Spatial/Styles/leaflet.min.css", "/OrchardCore.Spatial/Styles/leaflet.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.3.1/leaflet.css", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.3.1/leaflet.css")
                .SetCdnIntegrity("sha256-iYUgmrapfDGvBrePJPrMWQZDcObdAcStKBpjP3Az+3s=", "sha256-iYUgmrapfDGvBrePJPrMWQZDcObdAcStKBpjP3Az+3s=")
                .SetVersion("1.7.1")
                ;
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
