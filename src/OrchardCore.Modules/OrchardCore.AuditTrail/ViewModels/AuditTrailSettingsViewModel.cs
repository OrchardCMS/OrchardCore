using System;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailSettingsViewModel
    {
        public bool EnableClientIpAddressLogging { get; set; }
        public AuditTrailCategorySettingsViewModel[] Categories { get; set; } = Array.Empty<AuditTrailCategorySettingsViewModel>();
        public string[] IgnoredContentTypeNames { get; set; } = Array.Empty<string>();
    }
}
