using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Templates;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("templatepreview-edit")
            .SetUrl("~/OrchardCore.Templates/Scripts/templatepreview.edit.min.js", "~/OrchardCore.Templates/Scripts/templatepreview.edit.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript("monaco-workaround")
            .SetUrl("~/OrchardCore.Templates/Scripts/monaco-workaround.min.js", "~/OrchardCore.Templates/Scripts/monaco-workaround.js")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
