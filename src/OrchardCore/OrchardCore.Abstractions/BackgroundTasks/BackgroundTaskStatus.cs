namespace OrchardCore.BackgroundTasks;

/// <summary>
/// The execution status of a background task on the current node.
/// </summary>
public enum BackgroundTaskStatus
{
    /// <summary>
    /// The task is not running and no run is pending, it waits for its next scheduled occurrence.
    /// </summary>
    Idle,

    /// <summary>
    /// A run was requested on demand, the task runs as soon as the background service picks it up.
    /// </summary>
    Queued,

    /// <summary>
    /// The task is currently running.
    /// </summary>
    Running,
}
