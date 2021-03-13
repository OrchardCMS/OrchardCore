using Microsoft.Extensions.Localization;
using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailCategoryDescriptor
    {
        public string Category { get; set; }
        public string ProviderName { get; set; }
        public LocalizedString LocalizedName { get; set; }
        public IEnumerable<AuditTrailEventDescriptor> Events { get; set; }
    }
}
