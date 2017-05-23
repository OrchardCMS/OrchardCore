using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Handlers;
using Orchard.Indexing;

namespace Orchard.Lucene.Handlers
{
    public class LuceneIndexingContentHandler : ContentHandlerBase
    {
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LuceneIndexingContentHandler> _logger;

        public LuceneIndexingContentHandler(
            LuceneIndexManager luceneIndexManager, 
            IServiceProvider serviceProvider,
            ILogger<LuceneIndexingContentHandler> logger)
        {
            _luceneIndexManager = luceneIndexManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
         
        public override void Published(PublishContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type

            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, context.ContentItem.ContentType);
            // Lazy resolution to prevent cyclic dependency 
            var contentItemIndexHandlers = _serviceProvider.GetRequiredService<IEnumerable<IContentItemIndexHandler>>();
            contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger).GetAwaiter().GetResult();

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
                _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
            }
        }

        public override void Removed(RemoveContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
            }
        }

        public override void Unpublished(PublishContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
            }
        }
    }
}