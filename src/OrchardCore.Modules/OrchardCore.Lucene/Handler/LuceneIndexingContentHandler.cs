using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Modules;

namespace OrchardCore.Lucene.Handlers
{
    public class LuceneIndexingContentHandler : ContentHandlerBase
    {
        private readonly List<ContentContextBase> _contexts = new List<ContentContextBase>();

        public LuceneIndexingContentHandler()
        {
        }

        public override Task PublishedAsync(PublishContentContext context) => AddContextAsync(context);
        public override Task CreatedAsync(CreateContentContext context) => AddContextAsync(context);
        public override Task UpdatedAsync(UpdateContentContext context) => AddContextAsync(context);
        public override Task RemovedAsync(RemoveContentContext context) => AddContextAsync(context);
        public override Task UnpublishedAsync(PublishContentContext context) => AddContextAsync(context);

        private Task AddContextAsync(ContentContextBase context)
        {
            if (_contexts.Count == 0)
            {
                var contexts = _contexts;

                // Using a local var prevents the lambda from holding a ref on this scoped service.
                ShellScope.AddDeferredTask(scope => IndexingAsync(scope, contexts));
            }

            _contexts.Add(context);

            return Task.CompletedTask;
        }

        private static async Task IndexingAsync(ShellScope scope, IEnumerable<ContentContextBase> contexts)
        {
            var contextsGroupById = contexts
               // Filter cancelled content items.
               .Where(c => !c.ContentItem.IsCancelled)
               // Multiple items may have been updated in the same scope.
               .GroupBy(c => c.ContentItem.ContentItemId, c => c)
               ;

            var luceneIndexManager = scope.ServiceProvider.GetRequiredService<LuceneIndexManager>();
            var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
            var contentItemIndexHandlers = scope.ServiceProvider.GetServices<IContentItemIndexHandler>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<LuceneIndexingContentHandler>>();

            // Get all contexts for each content item id.
            foreach (var idContexts in contextsGroupById)
            {
                // Only process the last context.
                var context = idContexts.Last();

                var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });

                await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), logger);

                foreach (var indexSettings in luceneIndexSettingsService.List())
                {
                    if (indexSettings.IndexedContentTypes.Contains(context.ContentItem.ContentType))
                    {
                        if (context is PublishContentContext publishContext)
                        {
                            // Check if it is really a publish context.
                            if (publishContext.PublishingItem != null)
                            {
                                luceneIndexManager.DeleteDocuments(indexSettings.IndexName, new string[] { context.ContentItem.ContentItemId });
                                luceneIndexManager.StoreDocuments(indexSettings.IndexName, new DocumentIndex[] { buildIndexContext.DocumentIndex });
                            }
                            // We go here for an unpublishing context.
                            else if (!indexSettings.IndexLatest)
                            {
                                luceneIndexManager.DeleteDocuments(indexSettings.IndexName, new string[] { context.ContentItem.ContentItemId });
                            }
                        }
                        else if (context is UpdateContentContext)
                        {
                            if (indexSettings.IndexLatest && context.ContentItem.Latest)
                            {
                                luceneIndexManager.DeleteDocuments(indexSettings.IndexName, new string[] { context.ContentItem.ContentItemId });
                                luceneIndexManager.StoreDocuments(indexSettings.IndexName, new DocumentIndex[] { buildIndexContext.DocumentIndex });
                            }
                        }
                        else if (context is CreateContentContext)
                        {
                            if (indexSettings.IndexLatest && context.ContentItem.Latest)
                            {
                                luceneIndexManager.StoreDocuments(indexSettings.IndexName, new DocumentIndex[] { buildIndexContext.DocumentIndex });
                            }
                        }
                        else if (context is RemoveContentContext removeContext)
                        {
                            if (removeContext.NoActiveVersionLeft)
                            {
                                luceneIndexManager.DeleteDocuments(indexSettings.IndexName, new string[] { context.ContentItem.ContentItemId });
                            }
                        }
                    }
                }
            }
        }
    }
}
