namespace OrchardCore.Notifications.Services;

internal static class NotificationLengthValidator
{
    public static void Validate(string type, string referenceId)
    {
        if (type?.Length > NotificationConstants.NotificationTypeLength)
        {
            throw new ArgumentException($"The notification type cannot be longer than {NotificationConstants.NotificationTypeLength} characters.");
        }

        if (referenceId?.Length > NotificationConstants.NotificationReferenceIdLength)
        {
            throw new ArgumentException($"The notification reference id cannot be longer than {NotificationConstants.NotificationReferenceIdLength} characters.");
        }
    }
}
