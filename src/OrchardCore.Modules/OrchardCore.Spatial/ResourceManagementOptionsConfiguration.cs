using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Spatial
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static readonly ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("leaflet")
                .SetUrl("/OrchardCore.Spatial/Scripts/leaflet/leaflet.js", "/OrchardCore.Spatial/Scripts/leaflet/leaflet-src.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.3/leaflet.js", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.3/leaflet-src.js")
                .SetCdnIntegrity("sha384-okbbMvvx/qfQkmiQKfd5VifbKZ/W8p1qIsWvE1ROPUfHWsDcC8/BnHohF7vPg2T6", "sha384-x7PoOOpgJGgUPTj6ajie0SBQfPZ8S2FtDd8L0gQn3s+Sz9dvzwoSWvqbltR97ThL")
                .SetVersion("1.9.3");

            _manifest
                .DefineStyle("leaflet")
                .SetUrl("/OrchardCore.Spatial/Styles/leaflet.min.css", "/OrchardCore.Spatial/Styles/leaflet.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.3/leaflet.min.css", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.3/leaflet.css")
                .SetCdnIntegrity("sha384-cTNLivltikBj6gZvv7PqNudNArGkBGBg1p7ZM56VRY0iSLEdQW8AVLZkMCHCcyc+", "sha384-o/2yZuJZWGJ4s/adjxVW71R+EO/LyCwdQfP5UWSgX/w87iiTXuvDZaejd3TsN7mf")
                .SetVersion("1.9.3");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
