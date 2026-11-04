using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core.Operations;

/// <summary>Executes lifecycle work and returns only after its transaction scope has completed.</summary>
public interface IIndexOperationExecutor
{
    /// <summary>Executes the requested lifecycle action in an isolated scope.</summary>
    Task<IndexProcessingResult> ExecuteAsync(string indexId, IndexLifecycleAction action);
}

internal sealed class IndexOperationExecutor : IIndexOperationExecutor
{
    public async Task<IndexProcessingResult> ExecuteAsync(string indexId, IndexLifecycleAction action)
    {
        IndexProcessingResult result = null;
        await ShellScope.UsingChildScopeAsync(async scope => result = await scope.ServiceProvider
            .GetRequiredService<IIndexLifecycleService>().ExecuteAsync(indexId, action));
        return result;
    }
}

/// <summary>Schedules, claims and records lifecycle operations without repeating already-started work.</summary>
public sealed class IndexOperationRunner
{
    private static readonly TimeSpan _observationTimeout = TimeSpan.FromMinutes(30);
    private readonly IndexOperationStore _store;
    private readonly IIndexOperationExecutor _executor;
    private readonly IClock _clock;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<IndexOperationRunner> _logger;

    /// <summary>Creates the runner from tenant persistence, execution and HTTP services.</summary>
    public IndexOperationRunner(IndexOperationStore store, IIndexOperationExecutor executor, IClock clock,
        IHttpContextAccessor http, ILogger<IndexOperationRunner> logger)
    {
        _store = store;
        _executor = executor;
        _clock = clock;
        _http = http;
        _logger = logger;
    }

    /// <summary>Records a request and schedules it after the current HTTP request completes.</summary>
    public async Task<IndexOperation> QueueAsync(string indexId, IndexLifecycleAction action)
    {
        if (_http.HttpContext is null || ShellScope.Current is null)
        {
            throw new InvalidOperationException("Index operations require an active tenant HTTP scope.");
        }
        var operation = await _store.CreateAsync(indexId, action);
        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("IndexOperation_" + operation.OperationId,
            operation.OperationId, (scope, id) => scope.ServiceProvider.GetRequiredService<IndexOperationRunner>().RunAsync(id));
        return operation;
    }

    /// <summary>Reads status, marking overdue pending/running work uncertain without claiming it was cancelled.</summary>
    public async Task<IndexOperation> FindAsync(string operationId)
    {
        var operation = await _store.FindAsync(operationId);
        if (operation is not null && operation.State is IndexOperationState.Pending or IndexOperationState.Running
            && _clock.UtcNow - operation.UpdatedUtc >= _observationTimeout)
        {
            await _store.TransitionAsync(operationId, operation.State, IndexOperationState.Uncertain);
            operation = await _store.FindAsync(operationId);
        }
        return operation;
    }

    /// <summary>Claims pending work once and persists its confirmed outcome after execution commits.</summary>
    public async Task RunAsync(string operationId)
    {
        var operation = await FindAsync(operationId);
        if (operation?.State != IndexOperationState.Pending
            || !await _store.TransitionAsync(operationId, IndexOperationState.Pending, IndexOperationState.Running))
        {
            return;
        }
        IndexProcessingResult result;
        try
        {
            result = await _executor.ExecuteAsync(operation.IndexId, operation.Action);
            if (result is not null && result.IndexId != operation.IndexId)
            {
                throw new InvalidOperationException("The executor returned a result for a different index.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Index operation {OperationId} failed for index {IndexId}.", operationId, operation.IndexId);
            result = new IndexProcessingResult { IndexId = operation.IndexId, Status = IndexProcessingStatus.Failed };
        }
        var state = result?.Status switch
        {
            IndexProcessingStatus.Completed => IndexOperationState.Completed,
            IndexProcessingStatus.Unverified or IndexProcessingStatus.LockExpired => IndexOperationState.Uncertain,
            _ => IndexOperationState.Failed,
        };
        if (!await _store.TransitionAsync(operationId, IndexOperationState.Running, state, result)
            && state != IndexOperationState.Uncertain)
        {
            // An observation deadline may have elapsed while the original execution was still running.
            await _store.TransitionAsync(operationId, IndexOperationState.Uncertain, state, result);
        }
    }
}
