using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.PublishLater.ViewModels
{
    public class PublishLaterPartViewModel
    {
        [BindNever]
        public ContentItem ContentItem { get; set; }

        public DateTime? ScheduledPublishUtc { get; set; }

        public DateTime? ScheduledPublishLocalDateTime { get; set; }
    }
}
