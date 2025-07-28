using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.PublishLater.Models;

public class PublishLaterPart : ContentPart
{
    public DateTime? ScheduledPublishUtc { get; set; }

    [BindNever]
    public bool DoingScheduledPublish { get; set; }
}
