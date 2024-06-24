using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Themes.TheAdmin
{
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
                .DefineScript("admin")
                .SetDependencies("bootstrap", "admin-head", "jQuery")
                .SetUrl("~/TheAdmin/js/TheAdmin.min.js", "~/TheAdmin/js/TheAdmin.js")
                .SetVersion("1.0.0");

            manifest
                .DefineScript("admin-head")
                .SetUrl("~/TheAdmin/js/TheAdmin-header.min.js", "~/TheAdmin/js/TheAdmin-header.js")
                .SetDependencies("js-cookie", "theme-manager")
                .SetVersion("1.0.0");

            manifest
                .DefineScript("admin-main")
                .SetUrl("~/TheAdmin/js/TheAdmin-main.min.js", "~/TheAdmin/js/TheAdmin-main.js")
                .SetDependencies("admin-head")
                .SetVersion("1.0.0");

            manifest
                .DefineStyle("admin")
                .SetUrl("~/TheAdmin/css/TheAdmin.min.css", "~/TheAdmin/css/TheAdmin.css")
                .SetVersion("1.0.0");

            options.ResourceManifests.Add(manifest);
        }
    }
}
