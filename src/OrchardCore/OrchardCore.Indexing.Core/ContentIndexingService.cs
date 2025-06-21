using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Modules;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Indexing.Core;

public sealed class ContentIndexingService : NamedIndexingService
{
    private readonly IStore _store;
    private readonly IContentManager _contentManager;
    private readonly IEnumerable<IDocumentIndexHandler> _contentItemIndexHandlers;

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
        IEnumerable<IDocumentIndexHandler> contentItemIndexHandlers,
        ILogger<ContentIndexingService> logger)
        : base(
            IndexingConstants.ContentsIndexSource,
            indexProfileStore,
            indexingTaskManager,
            serviceProvider,
            logger)
    {
        _store = store;
        _contentManager = contentManager;
        _contentItemIndexHandlers = contentItemIndexHandlers;
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

    protected override async Task<BuildDocumentIndexContext> GetBuildDocumentIndexAsync(IndexProfileEntryContext entry, RecordIndexingTask task)
    {
        if (task.Type != RecordIndexingTaskTypes.Update)
        {
            return null;
        }

        var metadata = entry.IndexProfile.As<ContentIndexMetadata>();
        var anyCulture = string.IsNullOrEmpty(metadata.Culture) || metadata.Culture == "any";

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
            return null;
        }

        var buildIndexContext = new BuildDocumentIndexContext(new ContentItemDocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId), contentItem, [contentItem.ContentType], entry.DocumentIndexManager.GetContentIndexSettings());

        await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), Logger);

        // Ignore if the culture is not indexed in this index.
        if (!anyCulture)
        {
            if (!_cultureAspects.TryGetValue(contentItem.ContentItemVersionId ?? contentItem.ContentItemId, out var cultureAspect) && buildIndexContext.Record is ContentItem record)
            {
                cultureAspect = await _contentManager.PopulateAspectAsync<CultureAspect>(record);
                _cultureAspects[record.ContentItemVersionId ?? record.ContentItemId] = cultureAspect;
            }

            if (cultureAspect.Culture?.Name != metadata.Culture)
            {
                return null;
            }
        }

        return buildIndexContext;
    }
}
