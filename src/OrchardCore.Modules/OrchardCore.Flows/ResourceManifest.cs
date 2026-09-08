using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Flows;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest s_manifest;

    static ResourceManagementOptionsConfiguration()
    {
        s_manifest = new ResourceManifest();

        s_manifest
            .DefineStyle("flowpart-edit")
            .SetDependencies("widgetslist-edit")
            .SetUrl("~/OrchardCore.Flows/Styles/flows.edit.min.css", "~/OrchardCore.Flows/Styles/flows.edit.css");

        s_manifest
            .DefineScript("flowpart-edit")
            .SetDependencies("jQuery")
            .SetUrl("~/OrchardCore.Flows/Scripts/flows.edit.min.js", "~/OrchardCore.Flows/Scripts/flows.edit.js");

        s_manifest
            .DefineScript("content-type-picker")
            .SetDependencies("jQuery", "vuejs:2")
            .SetUrl("~/OrchardCore.Flows/Scripts/content-type-picker.min.js", "~/OrchardCore.Flows/Scripts/content-type-picker.js");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(s_manifest);
    }
}
