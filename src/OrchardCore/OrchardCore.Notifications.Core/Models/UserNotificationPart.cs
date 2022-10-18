using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications.Models;

public class UserNotificationPart : ContentPart
{
    public UserNotificationStrategy Strategy { get; set; }

    /// <summary>
    /// Sorted methods.
    /// </summary>
    public string[] Methods { get; set; }

    /// <summary>
    /// List of methods the user does not want to use.
    /// </summary>
    public string[] Optout { get; set; }
}
