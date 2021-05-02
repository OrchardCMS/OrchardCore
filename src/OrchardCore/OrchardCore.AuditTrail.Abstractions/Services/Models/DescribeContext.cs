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
                ProviderName = describe.ProviderName,
                LocalizedName = describe.Name,
                Events = describe.Events,
                Category = describe.Category
            });

        public DescribeFor For<T>(string category, LocalizedString name)
            where T : IAuditTrailEventProvider
        {
            var providerName = typeof(T).FullName;
            if (!_describes.TryGetValue($"{providerName}.{category}", out DescribeFor describeFor))
            {
                describeFor = new DescribeFor(category, providerName, name);
                _describes[$"{providerName}.{category}"] = describeFor;
            }

            return describeFor;
        }
    }
}
