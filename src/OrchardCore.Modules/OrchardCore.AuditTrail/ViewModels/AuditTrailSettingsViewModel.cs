using System;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailSettingsViewModel
    {
        public AuditTrailCategorySettingsViewModel[] Categories { get; set; } = Array.Empty<AuditTrailCategorySettingsViewModel>();
        public bool ClientIpAddressAllowed { get; set; }
    }
}
