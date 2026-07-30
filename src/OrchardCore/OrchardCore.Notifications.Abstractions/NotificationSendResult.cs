using Microsoft.Extensions.Localization;
using OrchardCore.Infrastructure;

namespace OrchardCore.Notifications;

public sealed class NotificationSendResult
{
    /// <summary>
    /// Gets or sets the number of notification methods that sent the notification successfully.
    /// </summary>
    public int SuccessfulCount { get; set; }

    /// <summary>
    /// Gets or sets the number of notification methods that failed to send the notification.
    /// </summary>
    public int FailedCount { get; set; }

    private List<ResultError> _errors;

    /// <summary>
    /// Gets the collection of errors associated with the result.
    /// </summary>
    public IReadOnlyList<ResultError> Errors => _errors ??= [];

    /// <summary>
    /// Adds an error to the notification send result.
    /// </summary>
    /// <param name="message">The localized error message.</param>
    /// <param name="key">The optional key associated with the error.</param>
    public void AddError(LocalizedString message, string key = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(message);

        _errors ??= [];
        _errors.Add(new ResultError
        {
            Key = key,
            Message = message,
        });
    }

    public NotificationSendStatus Status
    {
        get
        {
            var hasErrors = (_errors is not null && _errors.Count > 0) || FailedCount > 0;

            if (SuccessfulCount == 0 && hasErrors)
            {
                return NotificationSendStatus.Failed;
            }

            if (SuccessfulCount > 0 && !hasErrors)
            {
                return NotificationSendStatus.Success;
            }

            if (SuccessfulCount > 0 && hasErrors)
            {
                return NotificationSendStatus.PartiallySuccessful;
            }

            return NotificationSendStatus.None;
        }
    }
}

public enum NotificationSendStatus
{
    /// <summary>
    /// No notification methods were available to send the notification.
    /// </summary>
    None,

    /// <summary>
    /// All notification methods sent the notification successfully.
    /// </summary>
    Success,

    /// <summary>
    /// At least one notification method succeeded and at least one failed.
    /// </summary>
    PartiallySuccessful,

    /// <summary>
    /// All notification methods failed to send the notification.
    /// </summary>
    Failed,
}
