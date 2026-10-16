using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.Notifications.Models;

namespace OrchardCore.Notifications.ViewModels;

public class ListNotificationsViewModel
{
    public ListNotificationOptions Options { get; set; }

    /// <summary>
    /// The queried notifications on this page, before their row shapes are built.
    /// </summary>
    [BindNever]
    public IEnumerable<Notification> Notifications { get; set; }

    [BindNever]
    public IShape Header { get; set; }

    [BindNever]
    public IShape Pager { get; set; }

    /// <summary>
    /// The data context for rendering the notifications, the header and the pager in the configured layout.
    /// </summary>
    [BindNever]
    public AdminListContext List { get; set; }
}
