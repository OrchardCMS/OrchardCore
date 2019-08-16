using System;
using System.Collections.Generic;
using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Indexing;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Lucene.Handlers
{
    public class LuceneIndexingContentHandler : ContentHandlerBase
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LuceneIndexingContentHandler> _logger;

        public LuceneIndexingContentHandler(
            IContentDefinitionManager contentDefinitionManager,
            LuceneIndexManager luceneIndexManager,
            IServiceProvider serviceProvider,
            ILogger<LuceneIndexingContentHandler> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _luceneIndexManager = luceneIndexManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type

            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });
            // Lazy resolution to prevent cyclic dependency 
            var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();
            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
                _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
            }
        }

        public override async Task CreatedAsync(CreateContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type

            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });
            // Lazy resolution to prevent cyclic dependency 
            var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();
            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
            }
        }

        public override async Task UpdatedAsync(UpdateContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type
            //var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);

            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });
            // Lazy resolution to prevent cyclic dependency
            var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();
            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.DeleteDocumentVersions(index, new string[] { context.ContentItem.ContentItemVersionId });
                _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
            }
        }

        public override async Task VersionedAsync(VersionContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type

            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });
            // Lazy resolution to prevent cyclic dependency 
            var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();
            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.DeleteDocumentVersions(index, new string[] { context.ContentItem.ContentItemVersionId });
                _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
            }
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
            }

            return Task.CompletedTask;
        }

        public override async Task UnpublishedAsync(PublishContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type

            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });
            // Lazy resolution to prevent cyclic dependency 
            var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();
            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
                _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
            }
        }
    }
}