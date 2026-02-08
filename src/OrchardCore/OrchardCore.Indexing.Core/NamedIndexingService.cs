using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Indexing.Models;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core;

public abstract class NamedIndexingService
{
    protected readonly string Name;
    protected readonly ILogger Logger;

    private readonly IIndexProfileStore _indexProfileStore;
    private readonly IIndexingTaskManager _indexingTaskManager;
    private readonly IEnumerable<IDocumentIndexHandler> _documentIndexHandlers;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Gets the batch size for indexing operations. Can be overridden by derived classes to tune batch sizing.
    /// </summary>
    protected virtual int BatchSize => 100;

    protected NamedIndexingService(
        string name,
        IIndexProfileStore indexProfileStore,
        IIndexingTaskManager indexingTaskManager,
        IEnumerable<IDocumentIndexHandler> documentIndexHandlers,
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        _indexProfileStore = indexProfileStore;
        _indexingTaskManager = indexingTaskManager;
        _documentIndexHandlers = documentIndexHandlers;
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

        var distributedLock = _serviceProvider.GetRequiredService<IDistributedLock>();
        var lockers = new List<ILocker>();

        try
        {
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

                (var locker, var isLocked) = await distributedLock.TryAcquireLockAsync($"IndexingService-{indexProfile.Id}", TimeSpan.FromSeconds(3), TimeSpan.FromMinutes(15));

                if (!isLocked)
                {
                    documentIndexManagers.Remove(indexProfile.ProviderName);
                    indexManagers.Remove(indexProfile.ProviderName);
                    tracker.Remove(indexProfile.Id);

                    Logger.LogWarning("The index {Name} is already being indexed. Skipping", indexProfile.Name);

                    continue;
                }

                lockers.Add(locker);
            }

            if (tracker.Count == 0)
            {
                return;
            }

            while (true)
            {
                List<RecordIndexingTask> currentBatch = null;
                var batchProcessedSuccessfully = false;
                
                try
                {
                    // Load the next batch of tasks.
                    currentBatch = (await _indexingTaskManager.GetIndexingTasksAsync(lastTaskId, BatchSize, Name)).ToList();

                    if (currentBatch.Count == 0)
                    {
                        break;
                    }

                    // Group all DocumentIndex by index to batch update them.
                    var updatedDocumentsByIndex = tracker.Values.ToDictionary(x => x.IndexProfile.Id, b => new List<DocumentIndex>());

                    await BeforeProcessingTasksAsync(currentBatch, tracker.Values);

                    foreach (var entry in tracker.Values)
                    {
                        foreach (var task in currentBatch)
                        {
                            if (task.Id < entry.LastTaskId)
                            {
                                continue;
                            }

                            try
                            {
                                var buildIndexContext = await GetBuildDocumentIndexAsync(entry, task);

                                if (buildIndexContext is null)
                                {
                                    continue;
                                }

                                await _documentIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), Logger);

                                if (await ShouldTrackDocumentAsync(buildIndexContext, entry, task))
                                {
                                    updatedDocumentsByIndex[entry.IndexProfile.Id].Add(buildIndexContext.DocumentIndex);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log the error but continue processing remaining tasks
                                Logger.LogError(ex, "Error processing indexing task {TaskId} for index {IndexName}. Continuing with remaining tasks.", task.Id, entry.IndexProfile.Name);
                            }
                        }
                    }

                    lastTaskId = currentBatch.Last().Id;
                    batchProcessedSuccessfully = true;

                    foreach (var indexEntry in updatedDocumentsByIndex)
                    {
                        if (indexEntry.Value.Count == 0)
                        {
                            continue;
                        }

                        var trackerEntry = tracker[indexEntry.Key];

                        try
                        {
                            // AddOrUpdateDocumentsAsync is an upsert operation that handles both adding new documents
                            // and updating existing ones. Implementations should handle any necessary deletions internally.
                            if (await trackerEntry.DocumentIndexManager.AddOrUpdateDocumentsAsync(trackerEntry.IndexProfile, indexEntry.Value))
                            {
                                // We know none of the previous batches failed to update this index.
                                await trackerEntry.DocumentIndexManager.SetLastTaskIdAsync(trackerEntry.IndexProfile, lastTaskId);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log the error but continue processing remaining indexes
                            Logger.LogError(ex, "Error updating documents for index {IndexName}. Continuing with remaining indexes.", trackerEntry.IndexProfile.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log batch processing error and continue with next batch if possible
                    Logger.LogError(ex, "Error processing batch of indexing tasks. Attempting to continue with next batch.");
                    
                    // Move to next batch only if we haven't already updated lastTaskId and we successfully loaded tasks
                    if (!batchProcessedSuccessfully && currentBatch != null && currentBatch.Count > 0)
                    {
                        lastTaskId = currentBatch.Last().Id;
                    }
                    else if (currentBatch == null || currentBatch.Count == 0)
                    {
                        // If we couldn't load tasks, break the loop to avoid infinite retry
                        break;
                    }
                }
            }
        }
        finally
        {
            foreach (var locker in lockers)
            {
                await locker.DisposeAsync();
            }
        }
    }

    protected abstract Task<BuildDocumentIndexContext> GetBuildDocumentIndexAsync(IndexProfileEntryContext entry, RecordIndexingTask task);

    protected virtual ValueTask<bool> ShouldTrackDocumentAsync(BuildDocumentIndexContext buildIndexContext, IndexProfileEntryContext entry, RecordIndexingTask task)
        => ValueTask.FromResult(true);

    protected virtual Task BeforeProcessingTasksAsync(IEnumerable<RecordIndexingTask> tasks, IEnumerable<IndexProfileEntryContext> contexts)
        => Task.CompletedTask;
}
