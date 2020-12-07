using System;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailTrimmingSettings
    {
        public int RetentionPeriodDays { get; set; } = 10;
        public DateTime? LastRunUtc { get; set; }
        public bool Disabled { get; set; }
    }
}
