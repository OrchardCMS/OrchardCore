using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Lucene.Model;
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
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly LuceneIndexManager _indexManager;
        private readonly IIndexingTaskManager _indexingTaskManager;
        private readonly ISiteService _siteService;

        public LuceneIndexingService(
            IShellHost shellHost,
            ShellSettings shellSettings,
            LuceneIndexingState indexingState,
            LuceneIndexSettingsService luceneIndexSettingsService,
            LuceneIndexManager indexManager,
            IIndexingTaskManager indexingTaskManager,
            ISiteService siteService,
            ILogger<LuceneIndexingService> logger)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _indexingState = indexingState;
            _luceneIndexSettingsService = luceneIndexSettingsService;
            _indexManager = indexManager;
            _indexingTaskManager = indexingTaskManager;
            _siteService = siteService;

            Logger = logger;
        }

        public ILogger Logger { get; }

        public async Task ProcessContentItemsAsync(string indexName = default)
        {
            // TODO: Lock over the filesystem in case two instances get a command to rebuild the index concurrently.
            var allIndicesStatus = new Dictionary<string, int>();
            var lastTaskId = Int32.MaxValue;
            IDictionary indexSettingsList = null;

            if (String.IsNullOrEmpty(indexName))
            {
                indexSettingsList = _luceneIndexSettingsService.List().Where(x => x.IndexInBackgroundTask).ToImmutableDictionary(x => x.IndexName, x => x);

                if (indexSettingsList == null)
                {
                    return;
                }

                // Find the lowest task id to process
                foreach (DictionaryEntry item in indexSettingsList)
                {
                    var indexSetting = item.Value as LuceneIndexSettings;
                    var taskId = _indexingState.GetLastTaskId(indexSetting.IndexName);
                    lastTaskId = Math.Min(lastTaskId, taskId);
                    allIndicesStatus.Add(indexSetting.IndexName, taskId);
                }
            }
            else
            {
                indexSettingsList = _luceneIndexSettingsService.List().Where(x => x.IndexName == indexName).ToImmutableDictionary(x => x.IndexName, x => x);

                if (indexSettingsList == null)
                {
                    return;
                }

                var taskId = _indexingState.GetLastTaskId(indexName);
                lastTaskId = Math.Min(lastTaskId, taskId);
                allIndicesStatus.Add(indexName, taskId);
            }

            if (allIndicesStatus.Count == 0)
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

                    foreach (DictionaryEntry item in indexSettingsList)
                    {
                        var indexSettings = item.Value as LuceneIndexSettings;
                        if (indexSettings.IndexedContentTypes.Length == 0)
                        {
                            continue;
                        }

                        var contentItems = await contentManager.GetAsync(batch.Select(x => x.ContentItemId), indexSettings.IndexLatest);
                        contentItems = contentItems.Where(x => indexSettings.IndexedContentTypes.Contains(x.ContentType));

                        foreach (var task in batch)
                        {
                            var contentItem = contentItems.Where(x => x.ContentItemId == task.ContentItemId).FirstOrDefault();

                            if (contentItem == null)
                            {
                                continue;
                            }

                            // Ignore if this index has no content type setted to be indexed
                            if (!indexSettings.IndexedContentTypes.Contains(contentItem.ContentType))
                            {
                                continue;
                            }

                            var currentIndexStatus = allIndicesStatus.Where(x => x.Key == indexSettings.IndexName).FirstOrDefault();

                            if (currentIndexStatus.Value < task.Id)
                            {
                                _indexManager.DeleteDocuments(currentIndexStatus.Key, new string[] { task.ContentItemId });
                            }

                            if (task.Type == IndexingTaskTypes.Update)
                            {
                                var context = new BuildIndexContext(new DocumentIndex(task.ContentItemId), contentItem, new string[] { contentItem.ContentType });

                                // Update the document from the index if its lastIndexId is smaller than the current task id. 
                                await indexHandlers.InvokeAsync(x => x.BuildIndexAsync(context), Logger);

                                if (currentIndexStatus.Value < task.Id)
                                {
                                    _indexManager.StoreDocuments(currentIndexStatus.Key, new DocumentIndex[] { context.DocumentIndex });
                                }
                            }
                        }
                    }

                    // Update task ids
                    lastTaskId = batch.Last().Id;

                    foreach (var indexStatus in allIndicesStatus)
                    {
                        if (indexStatus.Value < lastTaskId)
                        {
                            _indexingState.SetLastTaskId(indexStatus.Key, lastTaskId);
                        }
                    }

                    _indexingState.Update();

                });
            } while (batch.Length == BatchSize);
        }

        /// <summary>
        /// Creates a new index
        /// </summary>
        /// <returns></returns>
        public void CreateIndex(LuceneIndexSettings indexSettings)
        {
            _luceneIndexSettingsService.CreateIndex(indexSettings);
            RebuildIndex(indexSettings.IndexName);
        }

        /// <summary>
        /// Edit an existing index
        /// </summary>
        /// <returns></returns>
        public void EditIndex(LuceneIndexSettings indexSettings)
        {
            _luceneIndexSettingsService.EditIndex(indexSettings);
        }

        /// <summary>
        /// Deletes permanently an index
        /// </summary>
        /// <returns></returns>
        public void DeleteIndex(LuceneIndexSettings indexSettings)
        {
            _indexManager.DeleteIndex(indexSettings.IndexName);
            _luceneIndexSettingsService.DeleteIndex(indexSettings);
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
