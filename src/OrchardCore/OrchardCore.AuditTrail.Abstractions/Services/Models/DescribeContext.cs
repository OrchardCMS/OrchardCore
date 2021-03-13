using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class DescribeContext
    {
        private readonly IDictionary<string, DescribeFor> _describes = new Dictionary<string, DescribeFor>();
        private readonly IList<Action<QueryFilterContext>> _queryFilters = new List<Action<QueryFilterContext>>();
        private readonly IList<Func<DisplayFilterContext, Task>> _filterDisplays = new List<Func<DisplayFilterContext, Task>>();

        public IEnumerable<Func<DisplayFilterContext, Task>> FilterDisplays => _filterDisplays;
        public IEnumerable<Action<QueryFilterContext>> QueryFilters => _queryFilters;

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

        public DescribeContext QueryFilter(Action<QueryFilterContext> queryAction)
        {
            _queryFilters.Add(queryAction);
            return this;
        }

        public DescribeContext DisplayFilter(Func<DisplayFilterContext, Task> displayFilter)
        {
            _filterDisplays.Add(displayFilter);
            return this;
        }
    }
}
