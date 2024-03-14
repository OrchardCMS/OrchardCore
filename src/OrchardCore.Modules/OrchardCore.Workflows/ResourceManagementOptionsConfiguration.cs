using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Workflows
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private static readonly ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("jsplumb")
                .SetDependencies("jQuery")
                .SetUrl("~/OrchardCore.Workflows/Scripts/jsplumb.min.js", "~/OrchardCore.Workflows/Scripts/jsplumb.js")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/jsPlumb/2.15.5/js/jsplumb.min.js", "https://cdnjs.cloudflare.com/ajax/libs/jsPlumb/2.15.5/js/jsplumb.js")
                .SetCdnIntegrity("sha384-vJ4MOlEjImsRl4La5sTXZ1UBtJ8uOOqxl2+0gdjRB7oVF6AvTVZ3woqYbTJb7vaf", "sha384-6qcVETlKUuSEc/QpsceL6BNiyEMBFSPE/uyfdRUvEfao8/K9lynY+r8nd/mwLGGh")
                .SetVersion("2.15.5");

            _manifest
                .DefineStyle("jsplumbtoolkit-defaults")
                .SetUrl("~/OrchardCore.Workflows/Styles/jsplumbtoolkit-defaults.min.css", "~/OrchardCore.Workflows/Styles/jsplumbtoolkit-defaults.css")
                .SetCdn("https://cdnjs.cloudflare.com/ajax/libs/jsPlumb/2.15.5/css/jsplumbtoolkit-defaults.min.css", "https://cdnjs.cloudflare.com/ajax/libs/jsPlumb/2.15.5/css/jsplumbtoolkit-defaults.css")
                .SetCdnIntegrity("sha384-4TTNOHwtAFYbq+UTSu/7Fj0xnqOabg7FYr9DkNtEKnmIx/YaACNiwhd2XZfO0A/u", "sha384-Q0wOomiqdBpz2z6/yYA8b3gc8A9t7z7QjD14d1WABvXVHbRYBu/IGOv3SOR57anB")
                .SetVersion("2.15.5");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }
    }
}
