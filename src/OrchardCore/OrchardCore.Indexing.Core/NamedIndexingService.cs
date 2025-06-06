using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core;

public abstract class NamedIndexingService
{
    protected readonly string Name;
    private readonly IIndexProfileStore _indexProfileStore;
    private readonly IIndexingTaskManager _indexingTaskManager;
    private readonly IServiceProvider _serviceProvider;

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly HashSet<string> _inProgressIndexes = [];

    private const int _batchSize = 100;

    protected NamedIndexingService(
        string name,
        IIndexProfileStore indexProfileStore,
        IIndexingTaskManager indexingTaskManager,
        IServiceProvider serviceProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        _indexProfileStore = indexProfileStore;
        _indexingTaskManager = indexingTaskManager;
        _serviceProvider = serviceProvider;
    }

    public async Task ProcessRecordsForAllIndexesAsync()
    {
        await ProcessRecordsAsync(await _indexProfileStore.GetByTypeAsync(Name));
    }

    public async Task ProcessRecordsAsync(IEnumerable<IndexProfile> indexes)
    {
        ArgumentNullException.ThrowIfNull(indexes);

        if (!indexes.Any())
        {
            return;
        }

        // This service could be called multiple times during the same request,
        // so we need to ensure that we only process each index once at a time.
        // This is not guaranteed to be thread-safe, but it should be sufficient for the current use case.
        await _semaphore.WaitAsync();

        var tracker = new Dictionary<string, IndexProfileEntryContext>();

        var documentIndexManagers = new Dictionary<string, IDocumentIndexManager>();

        var lastTaskId = long.MaxValue;

        try
        {
            // Find the lowest task id to process.
            foreach (var index in indexes)
            {
                if (index.Type != Name)
                {
                    // Skip indexes that are not content indexes.
                    continue;
                }

                if (!_inProgressIndexes.Add(index.Id))
                {
                    // If the index is already being processed, skip it.
                    continue;
                }

                if (!documentIndexManagers.TryGetValue(index.ProviderName, out var documentIndexManager))
                {
                    documentIndexManager = _serviceProvider.GetRequiredKeyedService<IDocumentIndexManager>(index.ProviderName);
                    documentIndexManagers.Add(index.ProviderName, documentIndexManager);
                }

                var taskId = await documentIndexManager.GetLastTaskIdAsync(index);
                lastTaskId = Math.Min(lastTaskId, taskId);
                tracker.Add(index.Id, new IndexProfileEntryContext(index, documentIndexManager, taskId));
            }
        }
        finally
        {
            _semaphore.Release();
        }

        if (tracker.Count == 0)
        {
            return;
        }

        var tasks = new List<IndexingTask>();

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

        foreach (var index in tracker.Values)
        {
            _inProgressIndexes.Remove(index.IndexProfile.Id);
        }
    }

    protected abstract Task<BuildDocumentIndexContext> GetBuildDocumentIndexAsync(IndexProfileEntryContext entry, IndexingTask task);

    protected virtual Task BeforeProcessingTasksAsync(IEnumerable<IndexingTask> tasks, IEnumerable<IndexProfileEntryContext> contexts)
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
