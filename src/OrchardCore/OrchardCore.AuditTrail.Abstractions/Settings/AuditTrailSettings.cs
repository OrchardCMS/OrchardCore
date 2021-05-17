using System;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailSettings
    {
        public AuditTrailCategorySettings[] Categories { get; set; } = Array.Empty<AuditTrailCategorySettings>();
        public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
        public bool ClientIpAddressAllowed { get; set; }
    }
}
