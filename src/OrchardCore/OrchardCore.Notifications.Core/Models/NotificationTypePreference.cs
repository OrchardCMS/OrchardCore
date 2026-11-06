namespace OrchardCore.Notifications.Models;

/// <summary>
/// The choices of a user for one type of notification, relative to the defaults of the type.
/// </summary>
public class NotificationTypePreference
{
    /// <summary>
    /// The methods the user turned on although the type turns them off by default.
    /// </summary>
    public string[] Optin { get; set; }

    /// <summary>
    /// The methods the user turned off for this type.
    /// </summary>
    public string[] Optout { get; set; }
}
