using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Indexing.Core;

/// <summary>Stores the failure counts in the tenant database, independently from the indexing work.</summary>
internal sealed class IndexingFailureStore : IIndexingFailureStore
{
    private readonly IStore _store;
    private readonly ILogger _logger;

    public IndexingFailureStore(IStore store, ILogger<IndexingFailureStore> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task<int> RecordFailureAsync(string indexId, long taskId)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexId);

        try
        {
            using var session = _store.CreateSession();
            var record = await FindAsync(session, indexId) ?? new IndexingFailureRecord { IndexId = indexId };
            var attempts = record.Attempts.GetValueOrDefault(taskId) + 1;
            record.Attempts[taskId] = attempts;
            await session.SaveAsync(record);
            await session.SaveChangesAsync();

            return attempts;
        }
        catch (Exception exception) when (!exception.IsFatal())
        {
            // Without a count the record is retried on the next run, which is the safe outcome.
            _logger.LogWarning(exception, "Unable to record the failure of indexing task {TaskId} for the index {IndexId}.", taskId, indexId);

            return 0;
        }
    }

    public async Task ClearAsync(string indexId)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexId);

        try
        {
            using var session = _store.CreateSession();
            var record = await FindAsync(session, indexId);

            if (record is not null)
            {
                session.Delete(record);
                await session.SaveChangesAsync();
            }
        }
        catch (Exception exception) when (!exception.IsFatal())
        {
            _logger.LogWarning(exception, "Unable to clear the indexing failures of the index {IndexId}.", indexId);
        }
    }

    // The records only exist while an index has a failing document, so there are too few of them to need
    // an index table. There is one per index as each is written under the lock of its index.
    private static async Task<IndexingFailureRecord> FindAsync(ISession session, string indexId)
        => (await session.Query<IndexingFailureRecord>().ListAsync()).FirstOrDefault(record => record.IndexId == indexId);
}

/// <summary>The tasks of an index that failed since its cursor last moved, with their number of attempts.</summary>
public sealed class IndexingFailureRecord
{
    /// <summary>Gets or sets the index profile identifier.</summary>
    public string IndexId { get; set; }

    /// <summary>Gets the number of failed attempts of each task identifier.</summary>
    public Dictionary<long, int> Attempts { get; init; } = [];
}
