using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Notifications.ViewModels;

public class UserNotificationViewModel
{
    public string[] Methods { get; set; }

    public string[] SortedMethods { get; set; }

    public string[] Optout { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> AvailableMethods { get; set; }
}
