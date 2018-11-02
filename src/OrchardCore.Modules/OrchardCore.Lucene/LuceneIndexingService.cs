using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Lucene
{
    /// <summary>
    /// This class provides services to update all the Lucene indices. It is non-rentrant so that calls 
    /// from different components can be done simultaneously, e.g. from a background task, an event or a UI interaction.
    /// It also indexes one content item at a time and provides the result to all indices.
    /// </summary>
    public class LuceneIndexingService
    {
        private const int BatchSize = 100;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly LuceneIndexingState _indexingState;
        private readonly LuceneIndexManager _indexManager;
        private readonly IIndexingTaskManager _indexingTaskManager;
        private readonly ISiteService _siteService;

        public LuceneIndexingService(
            IShellHost shellHost,
            ShellSettings shellSettings,
            LuceneIndexingState indexingState, 
            LuceneIndexManager indexManager, 
            IIndexingTaskManager indexingTaskManager,
            ISiteService siteService,
            ILogger<LuceneIndexingService> logger)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _indexingState = indexingState;
            _indexManager = indexManager;
            _indexingTaskManager = indexingTaskManager;
            _siteService = siteService;

            Logger = logger;
        }

        public ILogger Logger { get; }

        public async Task ProcessContentItemsAsync()
        {
            // TODO: Lock over the filesystem in case two instances get a command to rebuild the index concurrently.

            var allIndices = new Dictionary<string, int>();

            // Find the lowest task id to process
            int lastTaskId = int.MaxValue;
            foreach (var indexName in _indexManager.List())
            {
                var taskId = _indexingState.GetLastTaskId(indexName);
                lastTaskId = Math.Min(lastTaskId, taskId);
                allIndices.Add(indexName, taskId);
            }

            if (!allIndices.Any())
            {
                return;
            }

            IndexingTask[] batch;

            do
            {
                // Create a scope for the content manager
                using (var scope = await _shellHost.GetScopeAsync(_shellSettings))
                {
                    // Load the next batch of tasks
                    batch = (await _indexingTaskManager.GetIndexingTasksAsync(lastTaskId, BatchSize)).ToArray();

                    if (!batch.Any())
                    {
                        break;
                    }

                    foreach (var task in batch)
                    {
                        var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
                        var indexHandlers = scope.ServiceProvider.GetServices<IContentItemIndexHandler>();

                        foreach (var index in allIndices)
                        {
                            // TODO: ignore if this index is not configured for the content type

                            if (index.Value < task.Id)
                            {
                                _indexManager.DeleteDocuments(index.Key, new string[] { task.ContentItemId });
                            }
                        }

                        if (task.Type == IndexingTaskTypes.Update)
                        {
                            var contentItem = await contentManager.GetAsync(task.ContentItemId);

                            if (contentItem == null)
                            {
                                continue;
                            }

                            var context = new BuildIndexContext(new DocumentIndex(task.ContentItemId), contentItem, new string[] { contentItem.ContentType });

                            // Update the document from the index if its lastIndexId is smaller than the current task id. 
                            await indexHandlers.InvokeAsync(x => x.BuildIndexAsync(context), Logger);

                            foreach (var index in allIndices)
                            {
                                if (index.Value < task.Id)
                                {
                                    _indexManager.StoreDocuments(index.Key, new DocumentIndex[] { context.DocumentIndex });
                                }
                            }
                        }
                    }


                    // Update task ids
                    lastTaskId = batch.Last().Id;

                    foreach (var index in allIndices)
                    {
                        if (index.Value < lastTaskId)
                        {
                            _indexingState.SetLastTaskId(index.Key, lastTaskId);
                        }
                    }

                    _indexingState.Update();

                } 
            } while (batch.Length == BatchSize);
        }

        /// <summary>
        /// Restarts the indexing process from the beginning in order to update
        /// current content items. It doesn't delete existing entries from the index.
        /// </summary>
        public void ResetIndex(string indexName)
        {
            _indexingState.SetLastTaskId(indexName, 0);
            _indexingState.Update();
        }

        /// <summary>
        /// Deletes and recreates the full index content.
        /// </summary>
        public void RebuildIndex(string indexName)
        {
            _indexManager.DeleteIndex(indexName);
            _indexManager.CreateIndex(indexName);

            ResetIndex(indexName);
        }

        public async Task<LuceneSettings> GetLuceneSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            if (siteSettings.Has<LuceneSettings>())
            {
                return siteSettings.As<LuceneSettings>();
            }

            return null;
        }
    }
}
