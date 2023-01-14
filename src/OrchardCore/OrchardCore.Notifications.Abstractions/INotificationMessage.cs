namespace OrchardCore.Notifications;

public interface INotificationMessage
{
    string Summary { get; }
}

public interface INotificationBodyMessage
{
    string Body { get; }

    bool IsHtmlBody { get; }
}
