using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.DataPipelines;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("data-pipelines-designer")
            .SetUrl(
                "~/OrchardCore.DataPipelines/Scripts/DataPipelines/designer/data-pipelines-designer.min.js",
                "~/OrchardCore.DataPipelines/Scripts/DataPipelines/designer/data-pipelines-designer.js")
            .SetAttribute("type", "module")
            .SetVersion("1.0.0");

        _manifest
            .DefineStyle("data-pipelines-designer")
            .SetDependencies("bootstrap")
            .SetUrl(
                "~/OrchardCore.DataPipelines/Styles/data-pipelines-designer.min.css",
                "~/OrchardCore.DataPipelines/Styles/data-pipelines-designer.css")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript("data-pipelines-editors")
            .SetUrl(
                "~/OrchardCore.DataPipelines/Scripts/data-pipelines-editors.min.js",
                "~/OrchardCore.DataPipelines/Scripts/data-pipelines-editors.js")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
        => options.ResourceManifests.Add(_manifest);
}
