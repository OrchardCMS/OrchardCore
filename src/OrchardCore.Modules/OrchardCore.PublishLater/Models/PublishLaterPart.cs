using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.PublishLater.Models;

public class PublishLaterPart : ContentPart
{
    [Description("The UTC date and time when the content item is scheduled for publication, in ISO 8601 format.")]
    public DateTime? ScheduledPublishUtc { get; set; }
}
