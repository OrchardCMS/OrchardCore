using System.Collections.Generic;
using OrchardCore.Notifications;

namespace OrchardCore.Navigation.Core;

public class WebNotificationQueryResult
{
    public IEnumerable<WebNotification> Notifications { get; set; }
    public int TotalCount { get; set; }
}
