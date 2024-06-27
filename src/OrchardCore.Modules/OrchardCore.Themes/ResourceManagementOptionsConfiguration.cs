using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private readonly ShellSettings _shellSettings;

    public ResourceManagementOptionsConfiguration(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public void Configure(ResourceManagementOptions options)
    {
        var manifest = new ResourceManifest();

        manifest
            .DefineScript("theme-manager")
            .SetAttribute("data-tenant-name", _shellSettings.Name)
            .SetUrl("~/OrchardCore.Themes/Scripts/theme-manager.min.js", "~/OrchardCore.Themes/Scripts/theme-manager.js")
            .SetVersion("1.0.0");

        options.ResourceManifests.Add(manifest);
    }
}
