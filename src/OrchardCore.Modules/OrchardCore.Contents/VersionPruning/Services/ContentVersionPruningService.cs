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
    private const int BatchSize = 100;

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

        foreach (var contentItemId in candidateItemIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Load every archived version of this content item so the "keep N newest" rule is
            // evaluated over the full set, not only the aged versions.
            var versions = await _session
                .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentItemId == contentItemId &&
                    !x.Latest &&
                    !x.Published)
                .ListAsync(cancellationToken);

            var toDelete = ContentVersionPruningSelector.SelectForDeletion(versions, settings.VersionsToKeep, threshold);

            foreach (var version in toDelete)
            {
                try
                {
                    _session.Delete(version);
                    deleted++;

                    if (deleted % BatchSize == 0)
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
                .Take(BatchSize)
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

            if (count < BatchSize)
            {
                break;
            }
        }

        return contentItemIds;
    }
}
