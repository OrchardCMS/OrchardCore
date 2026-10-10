using System.Collections.Concurrent;

namespace OrchardCore.BackgroundTasks;

/// <summary>
/// Holds the schedulers of the background service, to expose their state and let tasks be run on demand.
/// </summary>
internal sealed class BackgroundTaskMonitor : IBackgroundTaskMonitor
{
    private TaskCompletionSource _runRequested = CreateRunRequestedSource();

    /// <summary>
    /// The schedulers of all tenants, keyed by <see cref="GetKey(string, string)"/>.
    /// </summary>
    public ConcurrentDictionary<string, BackgroundTaskScheduler> Schedulers { get; } = new();

    /// <summary>
    /// A task that completes when a run is requested, until <see cref="ResetRunRequested"/> is called.
    /// </summary>
    public Task RunRequested => Volatile.Read(ref _runRequested).Task;

    public Task<IReadOnlyList<BackgroundTaskState>> GetStatesAsync(string tenant)
    {
        IReadOnlyList<BackgroundTaskState> states = Schedulers.Values
            .Where(scheduler => scheduler.Tenant == tenant)
            .Select(scheduler => scheduler.GetState())
            .ToArray();

        return Task.FromResult(states);
    }

    public Task<BackgroundTaskState> GetStateAsync(string tenant, string name)
        => Task.FromResult(GetScheduler(tenant, name)?.GetState());

    public Task<BackgroundTaskRunRequestResult> RequestRunAsync(string tenant, string name)
    {
        var scheduler = GetScheduler(tenant, name);
        if (scheduler is null)
        {
            // The tasks of a tenant that just started are only loaded by the next iteration of the background
            // service, so it is also triggered to let a next request find the task sooner.
            Volatile.Read(ref _runRequested).TrySetResult();

            return Task.FromResult(BackgroundTaskRunRequestResult.NotFound);
        }

        var result = scheduler.RequestRun();
        if (result == BackgroundTaskRunRequestResult.Queued)
        {
            // The scheduler is queued before signaling, so a request is never lost when the signal is reset
            // just before, as the background service then checks the schedulers to run after resetting it.
            Volatile.Read(ref _runRequested).TrySetResult();
        }

        return Task.FromResult(result);
    }

    /// <summary>
    /// Resets <see cref="RunRequested"/> if a run was requested, before checking the schedulers to run.
    /// </summary>
    public void ResetRunRequested()
    {
        var current = Volatile.Read(ref _runRequested);
        if (current.Task.IsCompleted)
        {
            Interlocked.CompareExchange(ref _runRequested, CreateRunRequestedSource(), current);
        }
    }

    public static string GetKey(string tenant, string name) => tenant + name;

    private BackgroundTaskScheduler GetScheduler(string tenant, string name)
    {
        if (tenant is null || name is null)
        {
            return null;
        }

        if (Schedulers.TryGetValue(GetKey(tenant, name), out var scheduler) &&
            scheduler.Tenant == tenant &&
            scheduler.Name == name)
        {
            return scheduler;
        }

        return null;
    }

    private static TaskCompletionSource CreateRunRequestedSource()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
