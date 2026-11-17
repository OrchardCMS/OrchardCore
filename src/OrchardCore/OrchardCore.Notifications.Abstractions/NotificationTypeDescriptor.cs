using OrchardCore.Localization;

namespace OrchardCore.Notifications;

/// <summary>
/// Describes a type of notification sent by a module, see <see cref="Notification.Type"/>.
/// </summary>
/// <remarks>
/// A descriptor is shared by every request of the tenant: it has no dependencies and keeps no state.
/// </remarks>
public sealed class NotificationTypeDescriptor
{
    /// <summary>
    /// Creates a new instance of <see cref="NotificationTypeDescriptor"/>.
    /// </summary>
    /// <param name="name">The technical name of the type, stored on the notifications.</param>
    /// <param name="displayName">The name shown to users.</param>
    public NotificationTypeDescriptor(string name, LocalizationSource displayName)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(displayName);

        Name = name;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets the technical name of the type, stored on the notifications.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the name shown to users, in the type filter of the notification center and in their preferences.
    /// </summary>
    public LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets or sets an optional description shown to users in their notification preferences.
    /// </summary>
    public LocalizationSource Description { get; set; }

    /// <summary>
    /// Gets the notification methods, for example <c>Email</c> or <c>SMS</c>, that are turned off for this type
    /// until a user turns them on in their preferences. Every other method is on by default.
    /// </summary>
    public ISet<string> DisabledMethods { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
