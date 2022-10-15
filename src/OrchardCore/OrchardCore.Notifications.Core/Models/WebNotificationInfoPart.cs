using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.Notifications.Models;

public class WebNotificationMessagePart : ContentPart
{
    public bool IsRead { get; set; }

    public DateTime? ReadAtUtc { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }
}
