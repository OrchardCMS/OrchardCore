using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailCategoryDescriptor
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string ProviderName { get; set; }
        public LocalizedString LocalizedName { get; set; }
        public IEnumerable<AuditTrailEventDescriptor> Events { get; set; }
    }
}
