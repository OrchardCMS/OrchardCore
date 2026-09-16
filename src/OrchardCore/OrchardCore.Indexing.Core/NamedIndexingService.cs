using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Indexing.Models;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

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

    /// <summary>
    /// Processes the selected indexes and reports progress, unavailable providers, contention and failures.
    /// Completion means no further tasks were observed at the final queue read, not that future content changes are indexed.
    /// </summary>
    public async Task<IReadOnlyList<IndexProcessingResult>> ProcessRecordsWithResultsAsync(IEnumerable<string> indexIds)
    {
        ArgumentNullException.ThrowIfNull(indexIds);
        var ids = indexIds.Distinct(StringComparer.Ordinal).ToArray();
        var profiles = (await _indexProfileStore.GetByTypeAsync(Name)).Where(profile => ids.Contains(profile.Id)).ToArray();
        var results = (await ProcessRecordsAsync(profiles)).ToList();
        foreach (var id in ids.Except(profiles.Select(profile => profile.Id), StringComparer.Ordinal))
        {
            results.Add(new IndexProcessingResult { IndexId = id, Status = IndexProcessingStatus.NotFound });
        }
        return results;
    }

    internal async Task<IndexProcessingResult> ProcessIndexWithPreparationAsync(IndexProfile profile,
        Func<IndexProfile, IIndexManager, Action, Task<bool>> prepare)
        => (await ProcessRecordsAsync([profile], prepare)).Single();

    private async Task<IReadOnlyList<IndexProcessingResult>> ProcessRecordsAsync(IEnumerable<IndexProfile> indexProfiles,
        Func<IndexProfile, IIndexManager, Action, Task<bool>> prepare = null)
    {
        if (!indexProfiles.Any())
        {
            return [];
        }

        var results = indexProfiles.ToDictionary(profile => profile.Id,
            profile => new IndexProcessingResult { IndexId = profile.Id, Status = IndexProcessingStatus.Failed });
        var tracker = new Dictionary<string, IndexProfileEntryContext>();

        var documentIndexManagers = new Dictionary<string, IDocumentIndexManager>();
        var indexManagers = new Dictionary<string, IIndexManager>();

        var lastTaskId = long.MaxValue;

        var distributedLock = _serviceProvider.GetRequiredService<IDistributedLock>();
        var lockers = new List<ILocker>();
        var leases = new Dictionary<string, IndexingLease>();

        try
        {
            // Find the lowest task id to process.
            foreach (var indexProfile in indexProfiles)
            {
                if (indexProfile.Type != Name)
                {
                    results[indexProfile.Id].Status = IndexProcessingStatus.Unsupported;
                    continue;
                }

                if (!documentIndexManagers.TryGetValue(indexProfile.ProviderName, out var documentIndexManager))
                {
                    documentIndexManager = _serviceProvider.GetKeyedService<IDocumentIndexManager>(indexProfile.ProviderName);

                    if (documentIndexManager is null)
                    {
                        results[indexProfile.Id].Status = IndexProcessingStatus.ProviderUnavailable;
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
                        results[indexProfile.Id].Status = IndexProcessingStatus.ProviderUnavailable;
                        Logger.LogWarning("Unable to find an implementation of {Implementation} for the provider '{ProviderName}'", nameof(IIndexManager), indexProfile.ProviderName);

                        continue;
                    }

                    indexManagers.Add(indexProfile.ProviderName, indexManager);
                }

                var lease = new IndexingLease(_serviceProvider);
                (var locker, var isLocked) = await distributedLock.TryAcquireLockAsync($"IndexingService-{indexProfile.Id}", TimeSpan.FromSeconds(3), IndexingLease.Duration);

                if (!isLocked)
                {
                    results[indexProfile.Id].Status = IndexProcessingStatus.Busy;
                    Logger.LogWarning("The index {Name} is already being indexed. Skipping", indexProfile.Name);

                    continue;
                }

                lockers.Add(locker);
                leases.Add(indexProfile.Id, lease);
                try
                {
                    lease.EnsureActive();

                    if (prepare is not null && !await prepare(indexProfile, indexManager, lease.EnsureActive))
                    {
                        results[indexProfile.Id].Status = IndexProcessingStatus.ProviderRejected;
                        continue;
                    }

                    lease.EnsureActive();
                    if (!await indexManager.ExistsAsync(indexProfile.IndexFullName))
                    {
                        results[indexProfile.Id].Status = IndexProcessingStatus.ProviderMissing;
                        Logger.LogWarning("The index '{IndexName}' does not exist for the provider '{ProviderName}'.", indexProfile.IndexName, indexProfile.ProviderName);

                        continue;
                    }

                    lease.EnsureActive();
                    var taskId = await documentIndexManager.GetLastTaskIdAsync(indexProfile);
                    lease.EnsureActive();
                    results[indexProfile.Id].LastTaskId = taskId;
                    lastTaskId = Math.Min(lastTaskId, taskId);
                    tracker.Add(indexProfile.Id, new IndexProfileEntryContext(indexProfile, documentIndexManager, taskId));
                }
                catch (Exception) when (lease.Expired)
                {
                    results[indexProfile.Id].Status = IndexProcessingStatus.LockExpired;
                }
            }

            if (tracker.Count == 0)
            {
                return results.Values.ToArray();
            }

            while (tracker.Count > 0)
            {
                List<RecordIndexingTask> currentBatch = null;

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

                    var failedIndexes = new HashSet<string>();
                    await BeforeProcessingTasksAsync(currentBatch, tracker.Values);

                    foreach (var entry in tracker.Values)
                    {
                        foreach (var task in currentBatch)
                        {
                            if (task.Id <= entry.LastTaskId)
                            {
                                continue;
                            }

                            try
                            {
                                leases[entry.IndexProfile.Id].EnsureActive();
                                var buildIndexContext = await GetBuildDocumentIndexAsync(entry, task);

                                if (buildIndexContext is null)
                                {
                                    continue;
                                }

                                foreach (var handler in _documentIndexHandlers)
                                {
                                    leases[entry.IndexProfile.Id].EnsureActive();
                                    await handler.BuildIndexAsync(buildIndexContext);
                                }

                                if (await ShouldTrackDocumentAsync(buildIndexContext, entry, task))
                                {
                                    updatedDocumentsByIndex[entry.IndexProfile.Id].Add(buildIndexContext.DocumentIndex);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Keep this index at its previous cursor so the failed batch can be retried.
                                Logger.LogError(ex, "Error processing indexing task {TaskId} for index {IndexName}. Stopping this index until the next run.", task.Id, entry.IndexProfile.Name);
                                failedIndexes.Add(entry.IndexProfile.Id);
                                break;
                            }
                        }
                    }

                    lastTaskId = currentBatch.Last().Id;

                    foreach (var indexEntry in updatedDocumentsByIndex)
                    {
                        if (failedIndexes.Contains(indexEntry.Key))
                        {
                            continue;
                        }

                        var trackerEntry = tracker[indexEntry.Key];

                        try
                        {
                            leases[indexEntry.Key].EnsureActive();
                            // AddOrUpdateDocumentsAsync is an upsert operation that handles both adding new documents
                            // and updating existing ones. Implementations should handle any necessary deletions internally.
                            if (indexEntry.Value.Count == 0 || await trackerEntry.DocumentIndexManager.AddOrUpdateDocumentsAsync(trackerEntry.IndexProfile, indexEntry.Value))
                            {
                                leases[indexEntry.Key].EnsureActive();
                                // Successfully filtered records also count as processed, without regressing ahead indexes.
                                if (lastTaskId > trackerEntry.LastTaskId)
                                {
                                    await trackerEntry.DocumentIndexManager.SetLastTaskIdAsync(trackerEntry.IndexProfile, lastTaskId);
                                    leases[indexEntry.Key].EnsureActive();
                                    results[trackerEntry.IndexProfile.Id].LastTaskId = lastTaskId;
                                }
                            }
                            else
                            {
                                failedIndexes.Add(indexEntry.Key);
                                Logger.LogWarning("The provider rejected documents for index {IndexName}. Stopping this index until the next run.", trackerEntry.IndexProfile.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            failedIndexes.Add(indexEntry.Key);
                            Logger.LogError(ex, "Error updating documents for index {IndexName}. Stopping this index until the next run.", trackerEntry.IndexProfile.Name);
                        }
                    }

                    foreach (var id in failedIndexes)
                    {
                        tracker.Remove(id);
                    }
                }
                catch (Exception ex)
                {
                    // Do not skip a failed batch and later advance a cursor past it.
                    Logger.LogError(ex, "Error processing a batch of indexing tasks. Stopping until the next run.");
                    tracker.Clear();
                    break;
                }
            }
            foreach (var id in tracker.Keys)
            {
                results[id].Status = IndexProcessingStatus.Completed;
            }
            return results.Values.ToArray();
        }
        finally
        {
            foreach (var locker in lockers)
            {
                await locker.DisposeAsync();
            }
            foreach (var (id, lease) in leases)
            {
                if (lease.Expired) { results[id].Status = IndexProcessingStatus.LockExpired; }
            }
        }
    }

    protected abstract Task<BuildDocumentIndexContext> GetBuildDocumentIndexAsync(IndexProfileEntryContext entry, RecordIndexingTask task);

    protected virtual ValueTask<bool> ShouldTrackDocumentAsync(BuildDocumentIndexContext buildIndexContext, IndexProfileEntryContext entry, RecordIndexingTask task)
        => ValueTask.FromResult(true);

    protected virtual Task BeforeProcessingTasksAsync(IEnumerable<RecordIndexingTask> tasks, IEnumerable<IndexProfileEntryContext> contexts)
        => Task.CompletedTask;
}
