using OrchardCore.Notifications.Models;

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

public interface INotificationContentMessage
{
    NotificationLinkType LinkType { get; }

    string ContentItemId { get; }

    string ContentType { get; }

    string ContentOwnerId { get; }

    string CustomUrl { get; }
}
