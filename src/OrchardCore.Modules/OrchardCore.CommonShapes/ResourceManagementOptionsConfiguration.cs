using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.CommonShapes;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("phone-input")
            .SetUrl("~/OrchardCore.CommonShapes/Scripts/phone-input.min.js", "~/OrchardCore.CommonShapes/Scripts/phone-input.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle("phone-input")
            .SetUrl("~/OrchardCore.CommonShapes/Styles/phone-input.min.css", "~/OrchardCore.CommonShapes/Styles/phone-input.css")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
