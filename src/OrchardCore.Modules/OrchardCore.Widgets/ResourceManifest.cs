using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Widgets;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest s_manifest;

    static ResourceManagementOptionsConfiguration()
    {
        s_manifest = new ResourceManifest();

        s_manifest
            .DefineStyle("widgetslist-edit")
            .SetUrl("~/OrchardCore.Widgets/Styles/widgetslist.edit.min.css", "~/OrchardCore.Widgets/Styles/widgetslist.edit.css");

        s_manifest
            .DefineScript("widgetslist-edit")
            .SetDependencies("jQuery")
            .SetUrl("~/OrchardCore.Widgets/Scripts/widgetslist.edit.min.js", "~/OrchardCore.Widgets/Scripts/widgetslist.edit.js");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(s_manifest);
    }
}
