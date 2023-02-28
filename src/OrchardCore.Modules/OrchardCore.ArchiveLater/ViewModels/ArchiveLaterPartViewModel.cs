using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.ArchiveLater.ViewModels;

public class ArchiveLaterPartViewModel
{
    [BindNever]
    public ContentItem ContentItem { get; set; }

    public DateTime? ScheduledArchiveUtc { get; set; }

    public DateTime? ScheduledArchiveLocalDateTime { get; set; }
}
