namespace OrchardCore.Notifications;

public interface INotificationMessage
{
    string Subject { get; }

    string Body { get; }
}
