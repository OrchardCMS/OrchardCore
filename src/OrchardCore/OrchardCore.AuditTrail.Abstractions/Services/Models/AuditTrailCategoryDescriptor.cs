using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailCategoryDescriptor
    {
        public string Name { get; set; }
        public LocalizedString LocalizedName { get; set; }
        public IEnumerable<AuditTrailEventDescriptor> Events { get; set; } = Enumerable.Empty<AuditTrailEventDescriptor>();

        public static AuditTrailCategoryDescriptor Default(string name)
        {
            return new AuditTrailCategoryDescriptor
            {
                Name = name,
            };
        }
    }
}
