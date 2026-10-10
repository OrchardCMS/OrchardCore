namespace OrchardCore.BackgroundTasks;

/// <summary>
/// Describes a completed run of a background task.
/// </summary>
public sealed class BackgroundTaskRun
{
    /// <summary>
    /// The UTC date and time when the run started.
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// The UTC date and time when the run completed.
    /// </summary>
    public DateTime EndTime { get; init; }

    /// <summary>
    /// How long the run took.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// What started the run.
    /// </summary>
    public BackgroundTaskTrigger Trigger { get; init; }

    /// <summary>
    /// The outcome of the run.
    /// </summary>
    public BackgroundTaskRunResult Result { get; init; }

    /// <summary>
    /// The message of the exception thrown by the task, if the run failed.
    /// </summary>
    public string ErrorMessage { get; init; }

    /// <summary>
    /// The full description of the exception thrown by the task, including its type and stack trace, if the run failed.
    /// </summary>
    public string ErrorDetails { get; init; }
}
