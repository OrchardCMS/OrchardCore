using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.PublishLater.Models;

public class PublishLaterPart : ContentPart
{
    public DateTime? ScheduledPublishUtc { get; set; }
}
