using System;
using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailSettings
    {
        public bool ClientIpAddressAllowed { get; set; }
        public IList<AuditTrailEventSetting> EventSettings { get; set; } = new List<AuditTrailEventSetting>();
        public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    }
}
