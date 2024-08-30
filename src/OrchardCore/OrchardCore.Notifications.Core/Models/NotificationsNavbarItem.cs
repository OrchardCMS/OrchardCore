using OrchardCore.Entities;

namespace OrchardCore.Notifications.Models;

public sealed class NotificationsNavbarItem : Entity
{
    public string UserId { get; set; }

    public string Username { get; set; }

    public NotificationsNavbarItem(string userId, string username)
    {
        UserId = userId;
        Username = username;
    }
}
