using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ArchiveLater.Models;

public class ArchiveLaterPart : ContentPart
{
    [Description("The UTC date and time when the content item is scheduled for archival, in ISO 8601 format.")]
    public DateTime? ScheduledArchiveUtc { get; set; }
}
