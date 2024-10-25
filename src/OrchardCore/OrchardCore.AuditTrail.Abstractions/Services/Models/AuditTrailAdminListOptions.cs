using System.Collections.Frozen;

namespace OrchardCore.AuditTrail.Services.Models;

public class AuditTrailAdminListOptions
{
    internal Dictionary<string, AuditTrailAdminListOptionBuilder> SortOptionBuilders { get; set; } = [];

    private FrozenDictionary<string, AuditTrailAdminListOption> _sortOptions;

    public IReadOnlyDictionary<string, AuditTrailAdminListOption> SortOptions
        => _sortOptions ??= BuildSortOptions();

    private AuditTrailAdminListOption _defaultSortOption;
    public AuditTrailAdminListOption DefaultSortOption
        => _defaultSortOption ??= SortOptions.Values.FirstOrDefault(x => x.IsDefault);

    private FrozenDictionary<string, AuditTrailAdminListOption> BuildSortOptions()
    {
        var sortOptions = SortOptionBuilders.ToFrozenDictionary(k => k.Key, v => v.Value.Build());
        SortOptionBuilders = null;

        return sortOptions;
    }
}

public static class AuditTrailAdminListOptionsExtensions
{
    public static AuditTrailAdminListOptions ForSort(this AuditTrailAdminListOptions options, string value, Action<AuditTrailAdminListOptionBuilder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (!options.SortOptionBuilders.TryGetValue(value, out var auditTrailAdminListOptionBuilder))
        {
            auditTrailAdminListOptionBuilder = new AuditTrailAdminListOptionBuilder(value);
            options.SortOptionBuilders[value] = auditTrailAdminListOptionBuilder;
        }
        builder(auditTrailAdminListOptionBuilder);

        return options;
    }

    public static bool RemoveSort(this AuditTrailAdminListOptions options, string value)
        => options.SortOptionBuilders.Remove(value);
}
