namespace OrchardCore.Notifications;

/// <summary>
/// The criteria to find notifications with <see cref="INotificationManager"/>. The criteria that are set
/// are combined, and at least one of <see cref="UserId"/>, <see cref="Type"/> and <see cref="ReferenceId"/>
/// must be set.
/// </summary>
public sealed class NotificationFilter
{
    /// <summary>
    /// Gets or sets the id of the user the notifications were sent to.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Gets or sets the type of the notifications, see <see cref="Notification.Type"/>.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the id of the record the notifications are about, see <see cref="Notification.ReferenceId"/>.
    /// </summary>
    public string ReferenceId { get; set; }

    /// <summary>
    /// Gets or sets whether to only find read (<see langword="true"/>) or unread (<see langword="false"/>)
    /// notifications. Both are found when it is <see langword="null"/>.
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// Gets whether the filter sets at least one of <see cref="UserId"/>, <see cref="Type"/> and <see cref="ReferenceId"/>.
    /// </summary>
    public bool HasCriteria
        => !string.IsNullOrEmpty(UserId) || !string.IsNullOrEmpty(Type) || !string.IsNullOrEmpty(ReferenceId);
}
