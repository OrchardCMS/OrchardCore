using System;

namespace OrchardCore.PublishLater.ViewModels
{
    public class PublishLaterPartViewModel
    {
        public DateTime? ScheduledPublishUtc { get; set; }
        public DateTime? ScheduledPublishLocalDateTime { get; set; }
    }
}
