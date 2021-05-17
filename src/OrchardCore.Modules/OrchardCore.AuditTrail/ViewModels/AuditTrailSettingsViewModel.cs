using System;

namespace OrchardCore.AuditTrail.ViewModels
{
    public class AuditTrailSettingsViewModel
    {
        public AuditTrailCategorySettingsViewModel[] Categories { get; set; } = Array.Empty<AuditTrailCategorySettingsViewModel>();
        public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
        public bool ClientIpAddressAllowed { get; set; }
    }
}
