using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Forms
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        public const string EditFormWidgetOptions = "edit-form-widget-options";

        private static readonly ResourceManifest _manifest;

        static ResourceManagementOptionsConfiguration()
        {
            _manifest = new ResourceManifest();

            _manifest
                .DefineScript("edit-form-widget-options")
                .SetUrl("~/OrchardCore.Forms/Scripts/edit-form-widget-options.min.js", "~/OrchardCore.Forms/Scripts/edit-form-widget-options.js")
                .SetVersion("1.0.0");
        }

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(_manifest);
        }

        public static IResourceManager InjectEditFormWidgetOptions(IResourceManager resourceManager)
        {
            resourceManager.RegisterResource("script", EditFormWidgetOptions).AtFoot();

            return resourceManager;
        }
    }
}
