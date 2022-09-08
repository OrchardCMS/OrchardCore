using System;
using YesSql.Indexes;

namespace OrchardCore.ArchiveLater.Indexes;

public class ArchiveLaterPartIndex : MapIndex
{
    public string ContentItemId { get; set; }

    public DateTime? ScheduledArchiveDateTimeUtc { get; set; }

    public bool Published { get; set; }

    public bool Latest { get; set; }
}
