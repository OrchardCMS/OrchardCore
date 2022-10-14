using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications.Models;

public class UserNotificationPart : ContentPart
{
    public UserNotificationStrategy Strategy { get; set; }

    public string[] Methods { get; set; }
}
