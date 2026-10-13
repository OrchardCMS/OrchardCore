namespace OrchardCore.BackgroundTasks;

/// <summary>
/// Provides the execution state of the background tasks run by the background service of the current node,
/// and lets them be run on demand.
/// </summary>
/// <remarks>
/// This service is registered at the host level along with the background service, so it is not available
/// in an application that doesn't call <c>AddBackgroundService()</c>.
/// </remarks>
public interface IBackgroundTaskMonitor
{
    /// <summary>
    /// Gets the execution state of the background tasks of a tenant.
    /// </summary>
    /// <param name="tenant">The name of the tenant.</param>
    /// <returns>The states of the tasks handled by the background service, which doesn't include the tasks
    /// of a tenant that is not running yet.</returns>
    Task<IReadOnlyList<BackgroundTaskState>> GetStatesAsync(string tenant);

    /// <summary>
    /// Gets the execution state of a background task.
    /// </summary>
    /// <param name="tenant">The name of the tenant.</param>
    /// <param name="name">The technical name of the background task.</param>
    /// <returns>The state of the task, or <see langword="null"/> if the task is not handled by the background service.</returns>
    Task<BackgroundTaskState> GetStateAsync(string tenant, string name);

    /// <summary>
    /// Requests a background task to run as soon as possible, regardless of its schedule and of whether it is enabled,
    /// unless it is already running or queued. The run happens on the current node only.
    /// </summary>
    /// <param name="tenant">The name of the tenant.</param>
    /// <param name="name">The technical name of the background task.</param>
    /// <returns>Whether the run was queued, or why it was not.</returns>
    Task<BackgroundTaskRunRequestResult> RequestRunAsync(string tenant, string name);
}
