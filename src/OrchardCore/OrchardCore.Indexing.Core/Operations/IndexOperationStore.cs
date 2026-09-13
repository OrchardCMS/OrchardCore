using OrchardCore.Indexing.Core.Indexes;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Indexing.Core.Operations;

/// <summary>Persists operation state independently from the indexing transaction.</summary>
public sealed class IndexOperationStore
{
    private readonly IStore _store;
    private readonly IClock _clock;

    /// <summary>Creates a store using the current tenant's database and clock.</summary>
    public IndexOperationStore(IStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    /// <summary>Creates and commits a pending operation.</summary>
    public async Task<IndexOperation> CreateAsync(string indexId, IndexLifecycleAction action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexId);
        if (!Enum.IsDefined(action)) { throw new ArgumentOutOfRangeException(nameof(action)); }
        var operation = new IndexOperation
        {
            OperationId = Guid.NewGuid().ToString("N"), IndexId = indexId, Action = action,
            State = IndexOperationState.Pending, CreatedUtc = _clock.UtcNow, UpdatedUtc = _clock.UtcNow,
        };
        using var session = _store.CreateSession();
        await session.SaveAsync(operation);
        await session.SaveChangesAsync();
        return operation;
    }

    /// <summary>Reads an operation from this tenant, or returns null if absent.</summary>
    public async Task<IndexOperation> FindAsync(string operationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);
        using var session = _store.CreateSession();
        return await session.Query<IndexOperation, IndexOperationIndex>()
            .Where(index => index.OperationId == operationId).FirstOrDefaultAsync();
    }

    /// <summary>Commits a valid expected-state transition; returns false for missing or already-transitioned operations.</summary>
    public async Task<bool> TransitionAsync(string operationId, IndexOperationState expected, IndexOperationState next,
        IndexProcessingResult result = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);
        var valid = expected == IndexOperationState.Pending && next is IndexOperationState.Running or IndexOperationState.Failed or IndexOperationState.Uncertain
            || expected == IndexOperationState.Running && next is IndexOperationState.Completed or IndexOperationState.Failed or IndexOperationState.Uncertain
            || expected == IndexOperationState.Uncertain && next is IndexOperationState.Completed or IndexOperationState.Failed;
        if (!valid || (next == IndexOperationState.Completed && result?.Status != IndexProcessingStatus.Completed))
        {
            throw new InvalidOperationException("Invalid indexing operation transition.");
        }
        using var session = _store.CreateSession();
        var operation = await session.Query<IndexOperation, IndexOperationIndex>()
            .Where(index => index.OperationId == operationId).FirstOrDefaultAsync();
        if (operation is null || operation.State != expected) { return false; }
        if (result is not null && result.IndexId != operation.IndexId)
        {
            throw new InvalidOperationException("The processing result belongs to a different index.");
        }
        operation.State = next;
        operation.UpdatedUtc = _clock.UtcNow;
        operation.Outcome = result?.Status;
        operation.LastTaskId = result?.LastTaskId;
        try
        {
            await session.SaveAsync(operation, checkConcurrency: true);
            await session.SaveChangesAsync();
            return true;
        }
        catch (ConcurrencyException)
        {
            return false;
        }
    }
}
