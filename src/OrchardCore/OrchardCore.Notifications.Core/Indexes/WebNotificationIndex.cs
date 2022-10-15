using System;
using YesSql.Indexes;

namespace OrchardCore.Notifications.Indexes;

public class WebNotificationIndex : ReduceIndex
{
    public string ContentItemId { get; set; }

    public string UserId { get; set; }

    public int TotalUnread { get; set; }

    public DateTime? FirstMessageReceivedAt { get; set; }

    public DateTime? LastMessageReceivedAt { get; set; }
}
