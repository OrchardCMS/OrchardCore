using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.MiniProfiler;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest s_manifest;

    static ResourceManagementOptionsConfiguration()
    {
        s_manifest = new ResourceManifest();

        s_manifest
            .DefineStyle("miniprofiler-admin")
            .SetUrl("~/OrchardCore.MiniProfiler/Styles/miniprofiler-admin.min.css", "~/OrchardCore.MiniProfiler/Styles/miniprofiler-admin.css")
            .SetVersion("1.0.0");

        s_manifest
            .DefineScript("miniprofiler-admin")
            .SetUrl("~/OrchardCore.MiniProfiler/Scripts/miniprofiler-admin/miniprofiler-admin.min.js", "~/OrchardCore.MiniProfiler/Scripts/miniprofiler-admin/miniprofiler-admin.js")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(s_manifest);
    }
}
