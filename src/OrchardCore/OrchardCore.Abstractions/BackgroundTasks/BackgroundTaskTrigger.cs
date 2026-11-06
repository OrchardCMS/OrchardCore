namespace OrchardCore.BackgroundTasks;

/// <summary>
/// What started a background task run.
/// </summary>
public enum BackgroundTaskTrigger
{
    /// <summary>
    /// The run was started by the schedule of the task.
    /// </summary>
    Schedule,

    /// <summary>
    /// The run was requested on demand, for instance from the admin UI.
    /// </summary>
    Manual,
}
