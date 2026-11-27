using System.Collections;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.ContentLocalization.Indexes;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentLocalization.Services;

internal sealed class LocalizationSetGroupingProvider : IAdminListGroupingProvider
{
    private readonly ISession _session;
    private readonly IStringLocalizer S;

    public LocalizationSetGroupingProvider(ISession session, IStringLocalizer<LocalizationSetGroupingProvider> localizer)
    {
        _session = session;
        S = localizer;
    }

    public string Name => "LocalizationSet";
    public LocalizedString DisplayName => S["Localization set"];

    public async Task<IReadOnlyList<AdminListGroup>> GroupAsync(IEnumerable source, CancellationToken cancellationToken = default)
    {
        var items = source.Cast<ContentItemIndex>().ToArray();
        var membership = new Dictionary<long, string>();
        // Bound parameter counts on providers such as SQL Server, without loading content documents.
        foreach (var batch in items.Chunk(500))
        {
            var ids = batch.Select(item => item.DocumentId).ToArray();
            var entries = await _session.QueryIndex<LocalizationSetIndex>(index => index.DocumentId.IsIn(ids)).ListAsync(cancellationToken);
            foreach (var entry in entries)
            {
                membership.Add(entry.DocumentId, entry.LocalizationSet);
            }
        }

        return items.GroupBy(item => membership.TryGetValue(item.DocumentId, out var set) ? $"set:{set}" : $"item:{item.DocumentId}", StringComparer.Ordinal)
            .Select(group => new AdminListGroup
            {
                Key = group.Key,
                Title = string.IsNullOrEmpty(group.First().DisplayText) ? S["Untitled"].Value : group.First().DisplayText,
                Source = group.ToArray(),
                TotalCount = group.Count(),
            }).ToArray();
    }
}
