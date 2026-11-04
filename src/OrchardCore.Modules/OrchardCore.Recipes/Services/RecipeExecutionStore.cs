using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Recipes.Services;

/// <summary>The states of a recipe execution started by the management API.</summary>
internal enum RecipeExecutionState
{
    Running,
    Succeeded,
    Failed,

    /// <summary>The execution reported no outcome in time, so it may have been interrupted, or may still be running.</summary>
    Uncertain,
}

internal sealed record RecipeExecution
{
    public string ExecutionId { get; init; }
    public string RecipeId { get; init; }
    public string RecipeName { get; init; }
    public RecipeExecutionState State { get; init; }
    public string[] Errors { get; init; } = [];
    public DateTime StartedUtc { get; init; }
    public DateTime? CompletedUtc { get; init; }
}

/// <summary>The most recent recipe executions of a tenant, in a single document that can't grow further.</summary>
internal sealed class RecipeExecutionLog
{
    public List<RecipeExecution> Executions { get; init; } = [];
}

internal interface IRecipeExecutionStore
{
    /// <summary>Records a new execution. Returns false with the existing execution when the identifier is already used.</summary>
    Task<(bool Started, RecipeExecution Execution)> TryStartAsync(string executionId, string recipeId, string recipeName);

    Task<RecipeExecution> FindAsync(string executionId);

    Task CompleteAsync(string executionId, RecipeExecutionState state, string[] errors = null);
}

/// <summary>Commits the executions independently from the transactions of the recipe steps.</summary>
internal sealed class RecipeExecutionStore : IRecipeExecutionStore
{
    internal const int MaxExecutions = 100;

    // Long enough for a recipe to complete, as this can't tell if another request or node is still running it.
    internal static readonly TimeSpan OutcomeDeadline = TimeSpan.FromHours(1);

    private const int MaxAttempts = 3;

    private readonly IStore _store;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public RecipeExecutionStore(IStore store, IClock clock, ILogger<RecipeExecutionStore> logger)
    {
        _store = store;
        _clock = clock;
        _logger = logger;
    }

    public async Task<(bool Started, RecipeExecution Execution)> TryStartAsync(string executionId, string recipeId, string recipeName)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var session = _store.CreateSession();
                var log = await session.Query<RecipeExecutionLog>().FirstOrDefaultAsync() ?? new RecipeExecutionLog();
                var existing = log.Executions.Find(execution => execution.ExecutionId == executionId);

                if (existing is not null)
                {
                    return (false, Observe(existing));
                }

                var started = new RecipeExecution
                {
                    ExecutionId = executionId,
                    RecipeId = recipeId,
                    RecipeName = recipeName,
                    State = RecipeExecutionState.Running,
                    StartedUtc = _clock.UtcNow,
                };

                log.Executions.Add(started);
                Prune(log);
                await session.SaveAsync(log, checkConcurrency: true);
                await session.SaveChangesAsync();

                return (true, started);
            }
            catch (ConcurrencyException) when (attempt < MaxAttempts)
            {
                // Another execution changed the log, start over with its new version.
            }
        }
    }

    public async Task<RecipeExecution> FindAsync(string executionId)
    {
        using var session = _store.CreateSession();
        var log = await session.Query<RecipeExecutionLog>().FirstOrDefaultAsync();
        var execution = log?.Executions.Find(execution => execution.ExecutionId == executionId);

        return execution is null ? null : Observe(execution);
    }

    public async Task CompleteAsync(string executionId, RecipeExecutionState state, string[] errors = null)
    {
        try
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    using var session = _store.CreateSession();
                    var log = await session.Query<RecipeExecutionLog>().FirstOrDefaultAsync();
                    var index = log?.Executions.FindIndex(execution => execution.ExecutionId == executionId) ?? -1;

                    if (index >= 0)
                    {
                        log.Executions[index] = log.Executions[index] with { State = state, Errors = errors ?? [], CompletedUtc = _clock.UtcNow };
                        await session.SaveAsync(log, checkConcurrency: true);
                        await session.SaveChangesAsync();
                    }

                    return;
                }
                catch (ConcurrencyException) when (attempt < MaxAttempts)
                {
                    // Another execution changed the log, start over with its new version.
                }
            }
        }
        catch (Exception exception) when (!exception.IsFatal())
        {
            // The recipe is done, so this must not change its outcome. It ends up reported as uncertain.
            _logger.LogError(exception, "Unable to record the outcome of the recipe execution '{ExecutionId}'.", executionId);
        }
    }

    private RecipeExecution Observe(RecipeExecution execution)
        => execution.State == RecipeExecutionState.Running && execution.StartedUtc + OutcomeDeadline <= _clock.UtcNow
            ? execution with { State = RecipeExecutionState.Uncertain }
            : execution;

    private static void Prune(RecipeExecutionLog log)
    {
        var excess = log.Executions.Count - MaxExecutions;

        if (excess > 0)
        {
            // Oldest first, and the running ones last, as they are the ones a client is waiting for.
            var removed = log.Executions.OrderBy(execution => execution.State == RecipeExecutionState.Running)
                .ThenBy(execution => execution.StartedUtc)
                .Take(excess)
                .ToHashSet();
            log.Executions.RemoveAll(removed.Contains);
        }
    }
}
