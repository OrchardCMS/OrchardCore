using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Notifications.ViewModels;

public class UserNotificationViewModel
{
    public string[] Methods { get; set; }

    public string[] SortedMethods { get; set; }

    public string[] Optout { get; set; }

    /// <summary>
    /// The methods turned on per type of notification, by type name.
    /// </summary>
    public Dictionary<string, string[]> TypeMethods { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> AvailableMethods { get; set; }

    [BindNever]
    public IList<NotificationTypePreferenceViewModel> TypePreferences { get; set; } = [];
}

public class NotificationTypePreferenceViewModel
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string Description { get; set; }

    public IList<string> EnabledMethods { get; set; } = [];
}
