using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using OrchardCore.Notifications.Models;

namespace OrchardCore.Notifications.Drivers;

public class UserNotificationViewModel
{
    public UserNotificationStrategy Strategy { get; set; }

    public string[] Methods { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> AvailableMethods { get; set; }
}
