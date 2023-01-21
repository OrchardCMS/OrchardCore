namespace OrchardCore.Notifications.Models;

public enum NotificationStatus
{
    Read,
    Unread,
}

public enum NotificationOrder
{
    Latest,
    Oldest,
}

public enum NotificationBulkAction
{
    None,
    Read,
    Unread,
    Remove,
}
