using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.VersionPruning.Models;
using OrchardCore.Modules;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.VersionPruning.Services;

public class ContentVersionPruningService : IContentVersionPruningService
{
    // The number of versions deleted before changes are flushed to the database, so the
    // session's unit of work doesn't grow unbounded on sites with a large version history.
    private const int DeletionBatchSize = 100;

    // The number of content items whose versions are loaded per query. Loading several items
    // at once reduces the number of round-trips compared to querying one item at a time, while
    // still bounding how many versions are materialized in memory.
    private const int ContentItemBatchSize = 20;

    private readonly ISession _session;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public ContentVersionPruningService(
        ISession session,
        IClock clock,
        ILogger<ContentVersionPruningService> logger)
    {
        _session = session;
        _clock = clock;
        _logger = logger;
    }

    public async Task<int> PruneVersionsAsync(ContentVersionPruningSettings settings, CancellationToken cancellationToken = default)
    {
        // When no content types are selected there is nothing to prune.
        if (settings.ContentTypes is null || settings.ContentTypes.Length == 0)
        {
            return 0;
        }

        var threshold = _clock.UtcNow.AddDays(-settings.RetentionDays);

        // Identify the content items that have at least one archived (non-latest, non-published)
        // version older than the retention threshold. Only the lightweight index records are read
        // here so the whole version history is never materialized in memory at once.
        var candidateItemIds = await GetCandidateItemIdsAsync(settings.ContentTypes, threshold, cancellationToken);

        var deleted = 0;

        foreach (var itemIdsBatch in candidateItemIds.Chunk(ContentItemBatchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Load every archived version for this batch of content items in a single query so the
            // "keep N newest" rule is evaluated over the full set, not only the aged versions.
            var versions = await _session
                .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentItemId.IsIn(itemIdsBatch) &&
                    !x.Latest &&
                    !x.Published)
                .ListAsync(cancellationToken);

            foreach (var group in versions.GroupBy(x => x.ContentItemId))
            {
                var toDelete = ContentVersionPruningSelector.SelectForDeletion(group, settings.VersionsToKeep, threshold);

                foreach (var version in toDelete)
                {
                    try
                    {
                        _session.Delete(version);
                        deleted++;

                        if (deleted % DeletionBatchSize == 0)
                        {
                            await _session.FlushAsync(cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to prune content item {ContentItemId} version {ContentItemVersionId}.", version.ContentItemId, version.ContentItemVersionId);
                    }
                }
            }
        }

        return deleted;
    }

    private async Task<List<string>> GetCandidateItemIdsAsync(string[] contentTypes, DateTime threshold, CancellationToken cancellationToken)
    {
        var contentItemIds = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var processed = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var page = await _session
                .QueryIndex<ContentItemIndex>(x =>
                    !x.Latest &&
                    !x.Published &&
                    (x.ModifiedUtc == null || x.ModifiedUtc < threshold) &&
                    x.ContentType.IsIn(contentTypes))
                .OrderBy(x => x.ContentItemId)
                .ThenBy(x => x.DocumentId)
                .Skip(processed)
                .Take(ContentItemBatchSize)
                .ListAsync(cancellationToken);

            var count = 0;
            foreach (var index in page)
            {
                count++;

                if (seen.Add(index.ContentItemId))
                {
                    contentItemIds.Add(index.ContentItemId);
                }
            }

            processed += count;

            if (count < ContentItemBatchSize)
            {
                break;
            }
        }

        return contentItemIds;
    }
}
