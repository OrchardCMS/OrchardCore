namespace OrchardCore.Notifications.Models;

public enum UserNotificationStrategy
{
    /// <summary>
    /// Notify all notification providers.
    /// </summary>
    AllMethods,

    /// <summary>
    /// Notify using user's preferences until the first successful sent.
    /// </summary>
    UntilFirstSuccess
}
