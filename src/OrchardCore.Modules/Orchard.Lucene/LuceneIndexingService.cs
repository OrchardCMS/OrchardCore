using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement;
using Orchard.Indexing;
using Orchard.Settings;

namespace Orchard.Lucene
{
    /// <summary>
    /// This class provides services to update all the Lucene indexes. It is non-rentrant so that calls 
    /// from different components can be done simultaneously, e.g. from a background task, an event or a UI interaction.
    /// It also indexes one content item at a time and provides the result to all indexes.
    /// </summary>
    public class LuceneIndexingService
    {
        private const int BatchSize = 100;

        private readonly LuceneIndexingState _indexingState;
        private readonly LuceneIndexProvider _indexProvider;
        private readonly IIndexingTaskManager _indexTaskManager;
        private readonly IEnumerable<IContentItemIndexHandler> _indexHandlers;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;

        public LuceneIndexingService(
            LuceneIndexingState indexingState, 
            LuceneIndexProvider indexProvider, 
            IIndexingTaskManager indexTaskManager,
            IEnumerable<IContentItemIndexHandler> indexHandlers,
            IContentManager contentManager,
            ISiteService siteService,
            ILogger<LuceneIndexingService> logger)
        {
            _indexingState = indexingState;
            _indexProvider = indexProvider;
            _indexTaskManager = indexTaskManager;
            _indexHandlers = indexHandlers;
            _contentManager = contentManager;
            _siteService = siteService;

            Logger = logger;
        }

        public ILogger Logger { get; }

        public async Task ProcessContentItemsAsync()
        {
            // TODO: Lock over the filesystem

            var allIndexes = new Dictionary<string, int>();

            // Find the lowest task id to process
            int lastTaskId = int.MaxValue;
            foreach (var indexName in _indexProvider.List())
            {
                var taskId = _indexingState.GetLastTaskId(indexName);
                lastTaskId = Math.Min(lastTaskId, taskId);
                allIndexes.Add(indexName, taskId);
            }

            if (!allIndexes.Any())
            {
                return;
            }

            IEnumerable<IndexingTask> batch;

            do
            {
                // Load the next batch of tasks
                batch = (await _indexTaskManager.GetIndexingTasksAsync(lastTaskId, BatchSize)).ToArray();

                if (!batch.Any())
                {
                    break;
                }

                foreach (var task in batch)
                {
                    foreach (var index in allIndexes)
                    {
                        // TODO: ignore if this index is not configured for the content type

                        if (index.Value < task.Id)
                        {
                            _indexProvider.DeleteDocuments(index.Key, new string[] { task.ContentItemId });
                        }
                    }

                    if (task.Type == IndexingTaskTypes.Update)
                    {
                        var contentItem = await _contentManager.GetAsync(task.ContentItemId);
                        var context = new BuildIndexContext(new DocumentIndex(task.ContentItemId), contentItem, contentItem.ContentType);

                        // Update the document from the index if its lastIndexId is smaller than the current task id. 
                        await _indexHandlers.InvokeAsync(x => x.BuildIndexAsync(context), Logger);

                        foreach (var index in allIndexes)
                        {
                            if (index.Value < task.Id)
                            {
                                _indexProvider.StoreDocuments(index.Key, new DocumentIndex[] { context.DocumentIndex });
                            }
                        }
                    }
                }

                // Update task ids
                lastTaskId = batch.Last().Id;

                foreach (var index in allIndexes)
                {
                    if (index.Value < lastTaskId)
                    {
                        _indexingState.SetLastTaskId(index.Key, lastTaskId);
                    }
                }

                _indexingState.Update();

            } while (batch.Count() == BatchSize);
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
            _indexProvider.DeleteIndex(indexName);
            _indexProvider.CreateIndex(indexName);

            ResetIndex(indexName);
        }

        public async Task<LuceneSettings> GetLuceneSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            return siteSettings.As<LuceneSettings>();
        }
    }
}
