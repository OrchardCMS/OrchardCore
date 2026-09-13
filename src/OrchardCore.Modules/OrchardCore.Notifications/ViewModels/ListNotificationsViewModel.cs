using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Notifications.Models;

namespace OrchardCore.Notifications.ViewModels;

public class ListNotificationsViewModel
{
    public ListNotificationOptions Options { get; set; }

    [BindNever]
    public IEnumerable<dynamic> Notifications { get; set; }

    [BindNever]
    public dynamic Header { get; set; }

    [BindNever]
    public dynamic Pager { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the notifications, the header and the pager in the configured layout.
    /// </summary>
    [BindNever]
    public dynamic List { get; set; }
}
