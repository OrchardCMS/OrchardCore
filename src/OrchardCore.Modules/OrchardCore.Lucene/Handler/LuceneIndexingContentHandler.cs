using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Indexing;
using OrchardCore.Modules;

namespace OrchardCore.Lucene.Handlers
{
    public class LuceneIndexingContentHandler : ContentHandlerBase
    {
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly LuceneIndexSettings _luceneIndexSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LuceneIndexingContentHandler> _logger;

        public LuceneIndexingContentHandler(
            LuceneIndexManager luceneIndexManager,
            LuceneIndexSettings luceneIndexSettings,
            IServiceProvider serviceProvider,
            ILogger<LuceneIndexingContentHandler> logger)
        {
            _luceneIndexManager = luceneIndexManager;
            _luceneIndexSettings = luceneIndexSettings;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });
            // Lazy resolution to prevent cyclic dependency 
            var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();
            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var index in _luceneIndexManager.List())
            {
                var indexSettings = _luceneIndexSettings.List().Where(x => x.IndexName == index).FirstOrDefault();
                if (indexSettings.IndexedContentTypes.Contains(context.ContentItem.ContentType))
                {
                    _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
                    _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
                }
            }
        }

        public override async Task CreatedAsync(CreateContentContext context)
        {
            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });
            // Lazy resolution to prevent cyclic dependency 
            var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();
            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var index in _luceneIndexManager.List())
            {
                var indexSettings = _luceneIndexSettings.List().Where(x => x.IndexName == index).FirstOrDefault();
                if (indexSettings.IndexedContentTypes.Contains(context.ContentItem.ContentType))
                {
                    _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
                }
            }
        }

        public override async Task UpdatedAsync(UpdateContentContext context)
        {
            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });
            // Lazy resolution to prevent cyclic dependency
            var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();
            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var index in _luceneIndexManager.List())
            {
                var indexSettings = _luceneIndexSettings.List().Where(x => x.IndexName == index).FirstOrDefault();
                if (indexSettings.IndexedContentTypes.Contains(context.ContentItem.ContentType))
                {
                    if (indexSettings.IndexLatest)
                    {
                        _luceneIndexManager.DeleteDocumentVersions(index, new string[] { context.ContentItem.ContentItemVersionId });
                        _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
                    }
                }
            }
        }

        public override async Task VersionedAsync(VersionContentContext context)
        {
            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });
            // Lazy resolution to prevent cyclic dependency 
            var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();
            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var index in _luceneIndexManager.List())
            {
                var indexSettings = _luceneIndexSettings.List().Where(x => x.IndexName == index).FirstOrDefault();
                if (indexSettings.IndexedContentTypes.Contains(context.ContentItem.ContentType))
                {
                    if (indexSettings.IndexLatest)
                    {
                        _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
                    }
                    else {
                        _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
                        _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
                    }
                }
            }
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            foreach (var index in _luceneIndexManager.List())
            {
                var indexSettings = _luceneIndexSettings.List().Where(x => x.IndexName == index).FirstOrDefault();
                if (indexSettings.IndexedContentTypes.Contains(context.ContentItem.ContentType))
                {
                    _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
                }
            }

            return Task.CompletedTask;
        }

        public override async Task UnpublishedAsync(PublishContentContext context)
        {
            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });
            // Lazy resolution to prevent cyclic dependency 
            var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();
            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var index in _luceneIndexManager.List())
            {
                var indexSettings = _luceneIndexSettings.List().Where(x => x.IndexName == index).FirstOrDefault();
                if (indexSettings.IndexedContentTypes.Contains(context.ContentItem.ContentType))
                {
                    _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
                    _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
                }
            }
        }
    }
}