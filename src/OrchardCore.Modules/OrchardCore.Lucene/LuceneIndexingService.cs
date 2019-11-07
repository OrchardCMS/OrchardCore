using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
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
            var lastTaskId = int.MaxValue;
            foreach (var indexName in _indexManager.List())
            {
                var taskId = _indexingState.GetLastTaskId(indexName);
                lastTaskId = Math.Min(lastTaskId, taskId);
                allIndices.Add(indexName, taskId);
            }

            if (allIndices.Count == 0)
            {
                return;
            }

            var batch = Array.Empty<IndexingTask>();

            do
            {
                // Create a scope for the content manager
                var shellScope = await _shellHost.GetScopeAsync(_shellSettings);

                await shellScope.UsingAsync(async scope =>
                {
                    // Load the next batch of tasks
                    batch = (await _indexingTaskManager.GetIndexingTasksAsync(lastTaskId, BatchSize)).ToArray();

                    if (!batch.Any())
                    {
                        return;
                    }

                    var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
                    var indexHandlers = scope.ServiceProvider.GetServices<IContentItemIndexHandler>();

                    // Pre-load all content items to prevent SELECT N+1
                    var updatedContentItemIds = batch
                        .Where(x => x.Type == IndexingTaskTypes.Update)
                        .Select(x => x.ContentItemId)
                        .ToArray();

                    var allContentItems = await contentManager.GetAsync(updatedContentItemIds);

                    // Group all DocumentIndex by index to batch update them
                    var updatedDocumentsByIndex = new Dictionary<string, List<DocumentIndex>>();

                    foreach (var index in allIndices)
                    {
                        updatedDocumentsByIndex[index.Key] = new List<DocumentIndex>();
                    }

                    foreach (var task in batch)
                    {
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
                                    updatedDocumentsByIndex[index.Key].Add(context.DocumentIndex);
                                }
                            }
                        }
                    }

                    // Delete all the existing documents
                    foreach (var index in allIndices)
                    {
                        var deletedDocuments = batch
                            .Where(task => index.Value < task.Id)
                            .Select(task => task.ContentItemId)
                            .ToArray();

                        _indexManager.DeleteDocuments(index.Key, deletedDocuments);
                    }

                    // Submits all the new documents to the index
                    foreach (var index in allIndices)
                    {
                        _indexManager.StoreDocuments(index.Key, updatedDocumentsByIndex[index.Key]);
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

                });
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
