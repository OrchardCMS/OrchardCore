using OrchardCore.ResourceManagement;

namespace OrchardCore.Spatial
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();


            manifest
                .DefineScript("leaflet")
                .SetUrl("/OrchardCore.Spatial/leaflet/leaflet.js", "/OrchardCore.Spatial/leaflet/leaflet-src.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.3.1/leaflet.js", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.3.1/leaflet.js")
                .SetCdnIntegrity("sha256-CNm+7c26DTTCGRQkM9vp7aP85kHFMqs9MhPEuytF+fQ=", "sha256-CNm+7c26DTTCGRQkM9vp7aP85kHFMqs9MhPEuytF+fQ=")
                .SetVersion("1.3.1")
                ;

            manifest
                .DefineStyle("leaflet")
                .SetUrl("/OrchardCore.Spatial/leaflet/leaflet.css", "/OrchardCore.Spatial/leaflet/leaflet.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.3.1/leaflet.css", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.3.1/leaflet.css")
                .SetCdnIntegrity("sha256-iYUgmrapfDGvBrePJPrMWQZDcObdAcStKBpjP3Az+3s=", "sha256-iYUgmrapfDGvBrePJPrMWQZDcObdAcStKBpjP3Az+3s=")
                .SetVersion("1.3.1")
                ;
        }
    }
}