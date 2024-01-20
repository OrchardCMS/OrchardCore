using System.Collections.Generic;

namespace OrchardCore.Notifications.Models;

public class NotificationQueryResult
{
    public IEnumerable<Notification> Notifications { get; set; }

    public int TotalCount { get; set; }
}
