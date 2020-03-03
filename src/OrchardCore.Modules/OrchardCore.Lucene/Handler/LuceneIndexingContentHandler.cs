using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
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
            // A previewed content item is transient, and is marked as such with a negative id.
            if (context.ContentItem.Id == -1)
            {
                return Task.CompletedTask;
            }

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
            var services = scope.ServiceProvider;

            var contentManager = services.GetRequiredService<IContentManager>();
            var contentItemIndexHandlers = services.GetServices<IContentItemIndexHandler>();

            var luceneIndexManager = services.GetRequiredService<LuceneIndexManager>();
            var luceneIndexSettingsService = services.GetRequiredService<LuceneIndexSettingsService>();
            var logger = services.GetRequiredService<ILogger<LuceneIndexingContentHandler>>();

            // Multiple items may have been updated in the same scope, e.g through a recipe.
            var contextsGroupById = contexts.GroupBy(c => c.ContentItem.ContentItemId, c => c);

            // Get all contexts for each content item id.
            foreach (var ContextsById in contextsGroupById)
            {
                // Only process the last context.
                var context = ContextsById.Last();

                ContentItem published = null, latest = null;
                bool publishedLoaded = false, latestLoaded = false;

                foreach (var indexSettings in await luceneIndexSettingsService.GetSettingsAsync())
                {
                    if (indexSettings.IndexedContentTypes.Contains(context.ContentItem.ContentType))
                    {
                        if (!indexSettings.IndexLatest && !publishedLoaded)
                        {
                            publishedLoaded = true;
                            published = await contentManager.GetAsync(context.ContentItem.ContentItemId, VersionOptions.Published);
                        }

                        if (indexSettings.IndexLatest && !latestLoaded)
                        {
                            latestLoaded = true;
                            latest = await contentManager.GetAsync(context.ContentItem.ContentItemId, VersionOptions.Latest);
                        }

                        var contentItem = !indexSettings.IndexLatest ? published : latest;

                        if (contentItem == null)
                        {
                            await luceneIndexManager.DeleteDocumentsAsync(indexSettings.IndexName, new string[] { context.ContentItem.ContentItemId });
                        }
                        else
                        {
                            var buildIndexContext = new BuildIndexContext(new DocumentIndex(contentItem.ContentItemId), contentItem, new string[] { contentItem.ContentType });
                            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), logger);

                            await luceneIndexManager.DeleteDocumentsAsync(indexSettings.IndexName, new string[] { contentItem.ContentItemId });
                            await luceneIndexManager.StoreDocumentsAsync(indexSettings.IndexName, new DocumentIndex[] { buildIndexContext.DocumentIndex });
                        }
                    }
                }
            }
        }
    }
}
