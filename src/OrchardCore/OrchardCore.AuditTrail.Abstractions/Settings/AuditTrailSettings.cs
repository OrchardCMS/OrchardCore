using System;
using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailSettings
    {
        public IList<AuditTrailEventSettings> Events { get; set; } = new List<AuditTrailEventSettings>();
        public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
        public bool ClientIpAddressAllowed { get; set; }
    }
}
