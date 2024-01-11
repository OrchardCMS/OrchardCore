using System;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailTrimmingSettings
    {
        public int RetentionDays { get; set; } = 10;
        public DateTime? LastRunUtc { get; set; }
        public bool Disabled { get; set; }
    }
}
