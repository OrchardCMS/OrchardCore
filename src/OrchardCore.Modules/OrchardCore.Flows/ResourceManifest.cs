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
            .SetDependencies("Sortable")
            .SetUrl("~/OrchardCore.Flows/Scripts/flows/flows.edit.min.js", "~/OrchardCore.Flows/Scripts/flows/flows.edit.js");

        _manifest
            .DefineScript("content-type-picker")
            .SetDependencies("vuejs:2")
            .SetUrl("~/OrchardCore.Flows/Scripts/content-type-picker/content-type-picker.min.js", "~/OrchardCore.Flows/Scripts/content-type-picker/content-type-picker.js");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
