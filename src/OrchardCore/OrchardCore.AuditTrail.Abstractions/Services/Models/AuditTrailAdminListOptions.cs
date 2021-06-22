using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailAdminListOptions
    {
        internal Dictionary<string, AuditTrailAdminListOptionBuilder> SortOptionBuilders { get; set; } = new Dictionary<string, AuditTrailAdminListOptionBuilder>();

        private Dictionary<string, AuditTrailAdminListOption> _sortOptions;
        public IReadOnlyDictionary<string, AuditTrailAdminListOption> SortOptions => _sortOptions ??= BuildSortOptions();

        private AuditTrailAdminListOption _defaultSortOption;
        public AuditTrailAdminListOption DefaultSortOption => _defaultSortOption ??= SortOptions.Values.FirstOrDefault(x => x.IsDefault);

        private Dictionary<string, AuditTrailAdminListOption> BuildSortOptions()
        {
            var sortOptions = SortOptionBuilders.ToDictionary(k => k.Key, v => v.Value.Build());
            SortOptionBuilders = null;

            return sortOptions;
        }
    }

    public static class AuditTrailAdminListOptionsExtensions
    {
        public static AuditTrailAdminListOptionBuilder ForSort(this AuditTrailAdminListOptions options, string value)
        {
            if (options.SortOptionBuilders.ContainsKey(value))
            {
                throw new InvalidOperationException($"Sort '{value}' already registered");
            }
            var builder = new AuditTrailAdminListOptionBuilder(value);
            options.SortOptionBuilders[value] = builder;

            return builder;
        }

        public static bool RemoveSort(this AuditTrailAdminListOptions options, string value)
            => options.SortOptionBuilders.Remove(value);
    }
}
