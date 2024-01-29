using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Notifications;

public class NotificationOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static NotificationOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("notification-manager")
            .SetUrl("~/OrchardCore.Notifications/Scripts/notification-manager.min.js", "~/OrchardCore.Notifications/Scripts/notification-manager.js")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
