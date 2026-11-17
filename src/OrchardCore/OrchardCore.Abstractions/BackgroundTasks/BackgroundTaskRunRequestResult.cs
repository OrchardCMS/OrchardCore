namespace OrchardCore.BackgroundTasks;

/// <summary>
/// The result of a request to run a background task on demand.
/// </summary>
public enum BackgroundTaskRunRequestResult
{
    /// <summary>
    /// The run was queued and the task runs as soon as the background service picks it up.
    /// </summary>
    Queued,

    /// <summary>
    /// A run was already queued, the request has no further effect.
    /// </summary>
    AlreadyQueued,

    /// <summary>
    /// The task is already running, the request is ignored.
    /// </summary>
    AlreadyRunning,

    /// <summary>
    /// The task is not handled by the background service of the current node, for instance because its
    /// tenant is not running yet.
    /// </summary>
    NotFound,
}
