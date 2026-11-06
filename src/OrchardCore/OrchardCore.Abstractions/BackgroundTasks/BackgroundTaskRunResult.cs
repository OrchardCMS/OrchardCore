namespace OrchardCore.BackgroundTasks;

/// <summary>
/// The outcome of a background task run.
/// </summary>
public enum BackgroundTaskRunResult
{
    /// <summary>
    /// The task completed without throwing an exception.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The task threw an exception.
    /// </summary>
    Failed,

    /// <summary>
    /// The task was interrupted because the application was stopping.
    /// </summary>
    Canceled,
}
