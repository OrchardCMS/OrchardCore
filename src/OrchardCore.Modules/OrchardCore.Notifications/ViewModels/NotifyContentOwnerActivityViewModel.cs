using OrchardCore.Notifications.Models;

namespace OrchardCore.Notifications.ViewModels;

public class NotifyContentOwnerActivityViewModel : NotifyUserTaskActivityViewModel
{
    public NotificationLinkType LinkType { get; set; }

    public string Url { get; set; }
}
