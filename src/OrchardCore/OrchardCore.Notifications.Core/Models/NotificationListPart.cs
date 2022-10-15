using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications.Models;

public class NotificationListPart : ContentPart
{
    [BindNever]
    public string UserId { get; set; }

    [BindNever]
    public List<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
}
