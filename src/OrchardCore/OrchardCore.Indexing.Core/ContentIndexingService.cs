using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Indexing.Core;

public sealed class ContentIndexingService : NamedIndexingService
{
    private readonly IStore _store;
    private readonly IContentManager _contentManager;

    private ISession _readonlySession;
    private Dictionary<string, CultureAspect> _cultureAspects;
    private HashSet<string> _latestContentTypes;
    private HashSet<string> _publishedContentTypes;
    private Dictionary<string, ContentItem> _publishedContentItems;
    private Dictionary<string, ContentItem> _latestContentItems;

    public ContentIndexingService(
        IIndexingTaskManager indexingTaskManager,
        IIndexProfileStore indexProfileStore,
        IStore store,
        IContentManager contentManager,
        IServiceProvider serviceProvider,
        IEnumerable<IDocumentIndexHandler> documentIndexHandlers,
        ILogger<ContentIndexingService> logger)
        : base(
            IndexingConstants.ContentsIndexSource,
            indexProfileStore,
            indexingTaskManager,
            documentIndexHandlers,
            serviceProvider,
            logger)
    {
        _store = store;
        _contentManager = contentManager;
    }

    protected override async Task BeforeProcessingTasksAsync(IEnumerable<RecordIndexingTask> tasks, IEnumerable<IndexProfileEntryContext> contexts)
    {
        _readonlySession ??= _store.CreateSession(withTracking: false);
        _cultureAspects = new Dictionary<string, CultureAspect>();

        _latestContentTypes = new HashSet<string>();
        _publishedContentTypes = new HashSet<string>();

        foreach (var context in contexts)
        {
            var metadata = context.IndexProfile.As<ContentIndexMetadata>();

            if (metadata.IndexedContentTypes is null || metadata.IndexedContentTypes.Length == 0)
            {
                continue;
            }

            if (metadata.IndexLatest)
            {
                foreach (var contentType in metadata.IndexedContentTypes)
                {
                    _latestContentTypes.Add(contentType);
                }
            }
            else
            {
                foreach (var contentType in metadata.IndexedContentTypes)
                {
                    _publishedContentTypes.Add(contentType);
                }
            }
        }

        var updatedContentItemIds = tasks
            .Where(x => x.Type == RecordIndexingTaskTypes.Update)
            .Select(x => x.RecordId)
            .ToArray();

        _publishedContentItems = [];
        _latestContentItems = [];

        if (_publishedContentTypes.Count > 0)
        {
            var contentItems = await _readonlySession.Query<ContentItem, ContentItemIndex>(index => index.Published && index.ContentType.IsIn(_publishedContentTypes) && index.ContentItemId.IsIn(updatedContentItemIds))
                .ListAsync();

            _publishedContentItems = contentItems.DistinctBy(x => x.ContentItemId).ToDictionary(k => k.ContentItemId);
        }

        if (_latestContentTypes.Count > 0)
        {
            var contentItems = await _readonlySession.Query<ContentItem, ContentItemIndex>(index => index.Latest && index.ContentType.IsIn(_latestContentTypes) && index.ContentItemId.IsIn(updatedContentItemIds))
                .ListAsync();

            _latestContentItems = contentItems.DistinctBy(x => x.ContentItemId).ToDictionary(k => k.ContentItemId);
        }
    }

    protected override Task<BuildDocumentIndexContext> GetBuildDocumentIndexAsync(IndexProfileEntryContext entry, RecordIndexingTask task)
    {
        if (task.Type != RecordIndexingTaskTypes.Update)
        {
            return Task.FromResult<BuildDocumentIndexContext>(null);
        }

        var metadata = entry.IndexProfile.As<ContentIndexMetadata>();

        ContentItem contentItem = null;

        if (metadata.IndexLatest && _latestContentItems.TryGetValue(task.RecordId, out var latestContentItem) && metadata.IndexedContentTypes.Contains(latestContentItem.ContentType))
        {
            contentItem = latestContentItem;
        }

        if (contentItem is null && _publishedContentItems.TryGetValue(task.RecordId, out var publishedContentItem) && metadata.IndexedContentTypes.Contains(publishedContentItem.ContentType))
        {
            contentItem = publishedContentItem;
        }

        // We index only if we actually found a content item in the database.
        if (contentItem is null)
        {
            return Task.FromResult<BuildDocumentIndexContext>(null);
        }

        var context = new BuildDocumentIndexContext(new ContentItemDocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId), contentItem, [contentItem.ContentType], entry.DocumentIndexManager.GetContentIndexSettings());

        return Task.FromResult(context);
    }

    protected override async ValueTask<bool> ShouldTrackDocumentAsync(BuildDocumentIndexContext buildIndexContext, IndexProfileEntryContext entry, RecordIndexingTask task)
    {
        var metadata = entry.IndexProfile.As<ContentIndexMetadata>();
        var anyCulture = string.IsNullOrEmpty(metadata.Culture) || metadata.Culture == "any";

        // Ignore if the culture is not indexed in this index.
        if (!anyCulture)
        {
            var contentItem = (ContentItem)buildIndexContext.Record;
            if (!_cultureAspects.TryGetValue(contentItem.ContentItemVersionId ?? contentItem.ContentItemId, out var cultureAspect) && buildIndexContext.Record is ContentItem record)
            {
                cultureAspect = await _contentManager.PopulateAspectAsync<CultureAspect>(record);
                _cultureAspects[record.ContentItemVersionId ?? record.ContentItemId] = cultureAspect;
            }

            if (cultureAspect.Culture?.Name != metadata.Culture)
            {
                return false;
            }
        }

        return true;
    }
}
