using System.Collections.Generic;
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
}
