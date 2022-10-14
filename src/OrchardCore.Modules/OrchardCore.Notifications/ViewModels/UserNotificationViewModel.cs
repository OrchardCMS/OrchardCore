using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Notifications.Models;

namespace OrchardCore.Notifications.ViewModels;

public class UserNotificationViewModel
{
    public UserNotificationStrategy Strategy { get; set; }

    public string[] Methods { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> AvailableMethods { get; set; }
}
