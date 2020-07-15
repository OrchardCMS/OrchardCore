using System;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailTrimmingSettingsViewModel
    {
        public int RetentionPeriodDays { get; set; }
        public int MinimumRunIntervalHours { get; set; }
        public DateTime? LastRunUtc { get; set; }
        public bool Disabled { get; set; }
    }
}
