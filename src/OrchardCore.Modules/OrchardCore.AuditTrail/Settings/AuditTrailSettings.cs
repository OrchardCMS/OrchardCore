using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.AuditTrail.ViewModels;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailSettings
    {
        public bool EnableClientIpAddressLogging { get; set; }

        public IEnumerable<AuditTrailEventSetting> EventSettings { get; set; } = 
            Enumerable.Empty<AuditTrailEventSetting>();

        public IEnumerable<IgnoredContentTypesViewModel> IgnoredContentTypes { get; set; } = 
            Enumerable.Empty<IgnoredContentTypesViewModel>();

        public string[] IgnoredContentTypeNames { get; set; } = Array.Empty<string>();
    }
}
