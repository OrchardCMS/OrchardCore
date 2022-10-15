using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Notifications.ViewModels;

public class ListNotificationsViewModel
{
    [BindNever]
    public IEnumerable<dynamic> Notifications { get; set; }

    [BindNever]
    public dynamic Pager { get; set; }
}
