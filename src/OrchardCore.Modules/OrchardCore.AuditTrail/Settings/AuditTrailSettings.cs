using OrchardCore.AuditTrail.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailSettings
    {
        public bool EnableClientIpAddressLogging { get; set; }
        public IEnumerable<AuditTrailEventSetting> EventSettings { get; set; } = 
            Enumerable.Empty<AuditTrailEventSetting>();
        public IEnumerable<BlacklistedContentTypesViewModel> BlacklistedContentTypes { get; set; } = 
            Enumerable.Empty<BlacklistedContentTypesViewModel>();
        public string[] BlacklistedContentTypeNames { get; set; } = Array.Empty<string>();
    }
}
