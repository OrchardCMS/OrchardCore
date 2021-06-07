using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailAdminListOptions
    {
        private Dictionary<string, AuditTrailAdminListOptionBuilder> _sortOptionBuilders { get; } = new Dictionary<string, AuditTrailAdminListOptionBuilder>();

        private Dictionary<string, AuditTrailAdminListOption> _sortOptions;
        public Dictionary<string, AuditTrailAdminListOption> SortOptions => _sortOptions ??= _sortOptionBuilders.ToDictionary(k => k.Key, v => v.Value.Build());

        private AuditTrailAdminListOption _defaultSortOption;
        public AuditTrailAdminListOption DefaultSortOption => _defaultSortOption ??= SortOptions.Values.FirstOrDefault(x => x.IsDefault);

        public AuditTrailAdminListOptionBuilder ForSort(string value)
        {
            var builder = new AuditTrailAdminListOptionBuilder(value);
            _sortOptionBuilders[value] = builder;

            return builder;
        }
    }
}
