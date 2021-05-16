using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Providers;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DescribeContext
    {
        private readonly IDictionary<string, DescribeFor> _describes = new Dictionary<string, DescribeFor>();

        public IEnumerable<AuditTrailCategoryDescriptor> Describe() =>
            _describes.Values.Select(describe => new AuditTrailCategoryDescriptor
            {
                Name = describe.Category,
                ProviderName = describe.ProviderName,
                LocalizedName = describe.LocalizedName,
                Events = describe.Events
            });

        public DescribeFor For<T>(string category, LocalizedString localizedName) where T : IAuditTrailEventProvider
        {
            var providerName = typeof(T).FullName;

            var categoryFullName = providerName + category;
            if (!_describes.TryGetValue(categoryFullName, out var describeFor))
            {
                describeFor = new DescribeFor(category, categoryFullName, providerName, localizedName);
                _describes[categoryFullName] = describeFor;
            }

            return describeFor;
        }
    }
}
