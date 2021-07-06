using System;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailTrimmingSettingsViewModel
    {
        public int RetentionDays { get; set; }
        public DateTime? LastRunUtc { get; set; }
        public bool Disabled { get; set; }
    }
}
