namespace OrchardCore.Notifications;

public class NotificationConstants
{
    public const string NotificationCollection = "Notification";

    // Maximum length that MySql can support in an inner index under utf8mb4 collation is 768,
    // minus 2 for the 'DocumentId' integer (bigint size = 8 bytes = 2 character size),
    // minus 2 for the 'CreatedAtUtc' (date time size = 8 bytes = 2 character size),
    // minus 26 for 'NotificationId', 26 for 'UserId' and 1 for the 'IsRead' bool,
    // minus 4 to allow a new integer column, for example the 'Id' column,
    // minus 2 to allow a new date time, for example 'ReadAtUtc'.
    public const int NotificationIndexContentLength = 705;
}
