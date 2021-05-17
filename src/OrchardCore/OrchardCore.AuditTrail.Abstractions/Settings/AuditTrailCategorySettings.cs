using System;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailCategorySettings
    {
        public string Name { get; set; }
        public LocalizedString LocalizedName { get; set; }
        public AuditTrailEventSettings[] Events { get; set; } = Array.Empty<AuditTrailEventSettings>();
    }
}
