using System;
using YesSql.Indexes;

namespace OrchardCore.Notifications.Indexes;

public class WebNotificationIndex : MapIndex
{
    public string ContentItemId { get; set; }

    public string UserId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
