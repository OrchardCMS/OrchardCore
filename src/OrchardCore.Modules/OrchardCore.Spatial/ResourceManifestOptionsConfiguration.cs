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
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/leaflet.min.js", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/leaflet-src.js")
                .SetCdnIntegrity("sha384-vdvDM6Rl/coCrMsKwhal4uc9MUUFNrYa+cxp+nJQHy3TvozEpVKVexz/NTbE5VSO", "sha384-mc6rNK5V0bzWGJ1EUEAR2o+a/oH6qaVl+NCF63Et+mVpGnlSnyVSBhSP/wp4ir+O")
                .SetVersion("1.7.1");

            _manifest
                .DefineStyle("leaflet")
                .SetUrl("/OrchardCore.Spatial/Styles/leaflet.min.css", "/OrchardCore.Spatial/Styles/leaflet.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/leaflet.min.css", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/leaflet.css")
                .SetCdnIntegrity("sha384-d7pQbIswLsqVbYoAoHHlzPt+fmjkMwiXW/fvtIgK2r1u1bZXvGzL9HICUg4DKSgO", "sha384-VzLXTJGPSyTLX6d96AxgkKvE/LRb7ECGyTxuwtpjHnVWVZs2gp5RDjeM/tgBnVdM")
                .SetVersion("1.7.1");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
