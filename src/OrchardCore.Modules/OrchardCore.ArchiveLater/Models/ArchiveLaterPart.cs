using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.ArchiveLater.Models;

public class ArchiveLaterPart : ContentPart
{
    public DateTime? ScheduledArchiveUtc { get; set; }
}
