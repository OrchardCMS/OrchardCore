using System;
using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailSettings
    {
        public bool ClientIpAddressAllowed { get; set; }
        public IList<AuditTrailEventSettings> EventSettings { get; set; } = new List<AuditTrailEventSettings>();
        public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    }
}
