using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Flows;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineStyle("flowpart-edit")
            .SetDependencies("widgetslist-edit")
            .SetUrl("~/OrchardCore.Flows/Styles/flows.edit.min.css", "~/OrchardCore.Flows/Styles/flows.edit.css");

        _manifest
            .DefineScript("flowpart-edit")
            .SetDependencies("jQuery")
            .SetUrl("~/OrchardCore.Flows/Scripts/flows.edit.min.js", "~/OrchardCore.Flows/Scripts/flows.edit.js");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
