using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications.Models;

public class UserNotificationPreferencesPart : ContentPart
{
    /// <summary>
    /// Sorted methods.
    /// </summary>
    public string[] Methods { get; set; }

    /// <summary>
    /// List of methods the user does not want to use.
    /// </summary>
    public string[] Optout { get; set; }

    /// <summary>
    /// The choices of the user per type of notification, by type name. The methods in <see cref="Optout"/>
    /// stay off for every type.
    /// </summary>
    public Dictionary<string, NotificationTypePreference> Types { get; set; }
}
