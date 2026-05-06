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
    private readonly ISession _session;
    private readonly IClock _clock;
    private readonly ILogger<ContentVersionPruningService> _logger;

    public ContentVersionPruningService(
        ISession session,
        IClock clock,
        ILogger<ContentVersionPruningService> logger)
    {
        _session = session;
        _clock = clock;
        _logger = logger;
    }

    public async Task<int> PruneVersionsAsync(ContentVersionPruningSettings settings)
    {
        var candidates = await GetDeletionCandidatesAsync(settings);

        var deleted = 0;
        foreach (var version in candidates)
        {
            try
            {
                _session.Delete(version);
                deleted++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to prune content item {ContentItemId} version {ContentItemVersionId}", version.ContentItemId, version.ContentItemVersionId);
            }
        }

        if (candidates.Count != deleted && _logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Content version pruning count doesn't match - expected to delete {ToDeleteCount}, actually deleted only {Deleted}", candidates.Count, deleted);
        }

        return deleted;
    }

    private async Task<List<ContentItem>> GetDeletionCandidatesAsync(ContentVersionPruningSettings settings)
    {
        // Fetch all candidate archived versions (neither latest nor published)
        // that are older than the retention threshold. Versions with a null ModifiedUtc
        // are treated as oldest (they predate any known modification) and are always included.
        var dateThreshold = _clock.UtcNow.AddDays(-settings.RetentionDays);
        var query = _session
            .Query<ContentItem, ContentItemIndex>(x =>
                !x.Latest &&
                !x.Published &&
                (x.ModifiedUtc == null || x.ModifiedUtc < dateThreshold));

        var filterByType = settings.ContentTypes?.Length > 0;
        if (filterByType)
        {
            query = query.Where(x => x.ContentType.IsIn(settings.ContentTypes));
        }

        var candidates = await query.ListAsync();

        var versionsToKeep = Math.Max(0, settings.VersionsToKeep);

        return ContentVersionPruningSelector.SelectForDeletion(candidates, versionsToKeep);
    }
}
