namespace OrchardCore.Notifications;

/// <summary>
/// Finds, updates and deletes the notifications stored in the notification center after they are sent.
/// </summary>
/// <remarks>
/// Senders use it to keep their notifications in sync with the records they are about, for example to delete
/// the mention notifications of a comment when the comment is deleted. Every change refreshes the unread
/// notifications of the affected users.
/// </remarks>
public interface INotificationManager
{
    /// <summary>
    /// Gets the notification with the given id.
    /// </summary>
    /// <param name="notificationId">The id of the notification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The notification, or <see langword="null"/> when none has this id.</returns>
    Task<Notification> FindByIdAsync(string notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the notifications matching the given filter, the most recent first.
    /// </summary>
    /// <param name="filter">The filter, which must set at least one of its criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching notifications.</returns>
    Task<IReadOnlyList<Notification>> ListAsync(NotificationFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the changes made to a notification, for example a new subject, summary or URL.
    /// </summary>
    /// <param name="notification">The notification to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the notification is saved.</returns>
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    /// <param name="notification">The notification to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the notification is deleted.</returns>
    Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the notifications matching the given filter.
    /// </summary>
    /// <param name="filter">The filter, which must set at least one of its criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of deleted notifications.</returns>
    Task<int> DeleteAsync(NotificationFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="notification">The notification to mark as read.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when the notification was unread, <see langword="false"/> otherwise.</returns>
    Task<bool> MarkAsReadAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as unread.
    /// </summary>
    /// <param name="notification">The notification to mark as unread.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when the notification was read, <see langword="false"/> otherwise.</returns>
    Task<bool> MarkAsUnreadAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the unread notifications matching the given filter as read.
    /// </summary>
    /// <param name="filter">The filter, which must set at least one of its criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of notifications marked as read.</returns>
    Task<int> MarkAsReadAsync(NotificationFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether a notification has been read.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <returns><see langword="true"/> when the notification has been read.</returns>
    bool IsRead(Notification notification);
}
