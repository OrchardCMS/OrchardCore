using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core;

public abstract class NamedIndexingService
{
    protected readonly string Name;
    protected readonly ILogger Logger;

    private readonly IIndexProfileStore _indexProfileStore;
    private readonly IIndexingTaskManager _indexingTaskManager;
    private readonly IServiceProvider _serviceProvider;

    private const int _batchSize = 100;

    protected NamedIndexingService(
        string name,
        IIndexProfileStore indexProfileStore,
        IIndexingTaskManager indexingTaskManager,
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        _indexProfileStore = indexProfileStore;
        _indexingTaskManager = indexingTaskManager;
        _serviceProvider = serviceProvider;
        Logger = logger;
    }

    public async Task ProcessRecordsForAllIndexesAsync()
    {
        var indexProfiles = await _indexProfileStore.GetByTypeAsync(Name);

        await ProcessRecordsAsync(indexProfiles);
    }

    public async Task ProcessRecordsAsync(IEnumerable<string> indexIds)
    {
        ArgumentNullException.ThrowIfNull(indexIds);

        if (!indexIds.Any())
        {
            return;
        }

        var indexProfiles = await _indexProfileStore.GetByTypeAsync(Name);

        await ProcessRecordsAsync(indexProfiles.Where(x => indexIds.Contains(x.Id)));
    }

    private async Task ProcessRecordsAsync(IEnumerable<IndexProfile> indexProfiles)
    {
        if (!indexProfiles.Any())
        {
            return;
        }

        var tracker = new Dictionary<string, IndexProfileEntryContext>();

        var documentIndexManagers = new Dictionary<string, IDocumentIndexManager>();
        var indexManagers = new Dictionary<string, IIndexManager>();

        var lastTaskId = long.MaxValue;

        // Find the lowest task id to process.
        foreach (var indexProfile in indexProfiles)
        {
            if (indexProfile.Type != Name)
            {
                // Skip indexes that are not content indexes.
                continue;
            }

            if (!documentIndexManagers.TryGetValue(indexProfile.ProviderName, out var documentIndexManager))
            {
                documentIndexManager = _serviceProvider.GetKeyedService<IDocumentIndexManager>(indexProfile.ProviderName);

                if (documentIndexManager is null)
                {
                    Logger.LogWarning("Unable to find an implementation of {Implementation} for the provider '{ProviderName}'", nameof(IDocumentIndexManager), indexProfile.ProviderName);

                    continue;
                }

                documentIndexManagers.Add(indexProfile.ProviderName, documentIndexManager);
            }

            if (!indexManagers.TryGetValue(indexProfile.ProviderName, out var indexManager))
            {
                indexManager = _serviceProvider.GetKeyedService<IIndexManager>(indexProfile.ProviderName);

                if (indexManager is null)
                {
                    Logger.LogWarning("Unable to find an implementation of {Implementation} for the provider '{ProviderName}'", nameof(IIndexManager), indexProfile.ProviderName);

                    continue;
                }

                indexManagers.Add(indexProfile.ProviderName, indexManager);
            }

            if (!await indexManager.ExistsAsync(indexProfile.IndexFullName))
            {
                Logger.LogWarning("The index '{IndexName}' does not exist for the provider '{ProviderName}'.", indexProfile.IndexName, indexProfile.ProviderName);

                continue;
            }

            var taskId = await documentIndexManager.GetLastTaskIdAsync(indexProfile);
            lastTaskId = Math.Min(lastTaskId, taskId);
            tracker.Add(indexProfile.Id, new IndexProfileEntryContext(indexProfile, documentIndexManager, taskId));
        }

        if (tracker.Count == 0)
        {
            return;
        }

        var tasks = new List<RecordIndexingTask>();

        while (tasks.Count <= _batchSize)
        {
            // Load the next batch of tasks.
            tasks = (await _indexingTaskManager.GetIndexingTasksAsync(lastTaskId, _batchSize, Name)).ToList();

            if (tasks.Count == 0)
            {
                break;
            }

            // Group all DocumentIndex by index to batch update them.
            var updatedDocumentsByIndex = tracker.Values.ToDictionary(x => x.IndexProfile.Id, b => new List<DocumentIndex>());

            await BeforeProcessingTasksAsync(tasks, tracker.Values);

            foreach (var entry in tracker.Values)
            {
                foreach (var task in tasks)
                {
                    if (task.Id < entry.LastTaskId)
                    {
                        continue;
                    }

                    var buildIndexContext = await GetBuildDocumentIndexAsync(entry, task);

                    if (buildIndexContext is not null)
                    {
                        updatedDocumentsByIndex[entry.IndexProfile.Id].Add(buildIndexContext.DocumentIndex);
                    }
                }
            }

            lastTaskId = tasks.Last().Id;

            foreach (var indexEntry in updatedDocumentsByIndex)
            {
                if (indexEntry.Value.Count == 0)
                {
                    continue;
                }

                var trackerEntry = tracker[indexEntry.Key];

                // Delete all the deleted documents from the index.
                var deletedDocumentIds = indexEntry.Value.Select(x => x.Id);

                await trackerEntry.DocumentIndexManager.DeleteDocumentsAsync(trackerEntry.IndexProfile, deletedDocumentIds);

                // Upload documents to the index.
                if (await trackerEntry.DocumentIndexManager.AddOrUpdateDocumentsAsync(trackerEntry.IndexProfile, indexEntry.Value))
                {
                    // We know none of the previous batches failed to update this index.
                    await trackerEntry.DocumentIndexManager.SetLastTaskIdAsync(trackerEntry.IndexProfile, lastTaskId);
                }
            }
        }
    }

    protected abstract Task<BuildDocumentIndexContext> GetBuildDocumentIndexAsync(IndexProfileEntryContext entry, RecordIndexingTask task);

    protected virtual Task BeforeProcessingTasksAsync(IEnumerable<RecordIndexingTask> tasks, IEnumerable<IndexProfileEntryContext> contexts)
        => Task.CompletedTask;

    public sealed class IndexProfileEntryContext
    {
        public readonly IndexProfile IndexProfile;

        public long LastTaskId { get; }

        public readonly IDocumentIndexManager DocumentIndexManager;

        public IndexProfileEntryContext(IndexProfile indexProfile, IDocumentIndexManager documentIndexManager, long lastTaskId)
        {
            ArgumentNullException.ThrowIfNull(indexProfile);
            ArgumentNullException.ThrowIfNull(documentIndexManager);

            IndexProfile = indexProfile;
            DocumentIndexManager = documentIndexManager;
            LastTaskId = lastTaskId;
        }
    }
}
