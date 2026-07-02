using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Widgets;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineStyle("widgetslist-edit")
            .SetUrl("~/OrchardCore.Widgets/Styles/widgetslist.edit.min.css", "~/OrchardCore.Widgets/Styles/widgetslist.edit.css");

        _manifest
            .DefineScript("widgetslist-edit")
            .SetDependencies("Sortable")
            .SetUrl("~/OrchardCore.Widgets/Scripts/widgets/widgetslist.edit.min.js", "~/OrchardCore.Widgets/Scripts/widgets/widgetslist.edit.js");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
