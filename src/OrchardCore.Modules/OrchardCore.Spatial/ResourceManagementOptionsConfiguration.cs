using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Spatial;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("leaflet")
            .SetUrl("/OrchardCore.Spatial/Scripts/leaflet/leaflet.js", "/OrchardCore.Spatial/Scripts/leaflet/leaflet-src.js")
            .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/leaflet.js", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/leaflet-src.js")
            .SetCdnIntegrity("sha384-cxOPjt7s7Iz04uaHJceBmS+qpjv2JkIHNVcuOrM+YHwZOmJGBXI00mdUXEq65HTH", "sha384-4aETf8z71hiSsoK0xYsa5JtiJHfL3h7uMAsZ2QYOLvcySDL/cEDfdLt0SaBypTQZ")
            .SetVersion("1.9.4");

        _manifest
            .DefineStyle("leaflet")
            .SetUrl("/OrchardCore.Spatial/Styles/leaflet.min.css", "/OrchardCore.Spatial/Styles/leaflet.css")
            .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/leaflet.min.css", "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/leaflet.css")
            .SetCdnIntegrity("sha384-c6Rcwz4e4CITMbu/NBmnNS8yN2sC3cUElMEMfP3vqqKFp7GOYaaBBCqmaWBjmkjb", "sha384-sHL9NAb7lN7rfvG5lfHpm643Xkcjzp4jFvuavGOndn6pjVqS6ny56CAt3nsEVT4H")
            .SetVersion("1.9.4");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
