using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DescribeContext
    {
        private readonly IDictionary<string, DescribeFor> _describes = new Dictionary<string, DescribeFor>();

        public string Category { get; set; }

        public IEnumerable<AuditTrailCategoryDescriptor> Describe() =>
            _describes.Values.Select(describe => new AuditTrailCategoryDescriptor
            {
                Name = describe.Category,
                LocalizedName = describe.LocalizedName,
                Events = describe.Events
            });

        public DescribeFor For(string category, LocalizedString localizedName)
        {
            if (!_describes.TryGetValue(category, out var describeFor))
            {
                describeFor = new DescribeFor(category, localizedName);
                _describes[category] = describeFor;
            }

            return describeFor;
        }
    }
}
