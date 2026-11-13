namespace OrchardCore.Jobs.Models;

/// <summary>
/// How far a run got.
/// </summary>
public enum JobRunStatus
{
    /// <summary>
    /// The run waits for a server to execute it, at once or when it is due.
    /// </summary>
    Queued,

    /// <summary>
    /// A server executes the run.
    /// </summary>
    Running,

    /// <summary>
    /// The job is done, and the run waits for its child runs to complete.
    /// </summary>
    Waiting,

    /// <summary>
    /// The run completed, as did its child runs.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The run failed on its last attempt, its server stopped while executing it, or one of its child runs didn't
    /// succeed.
    /// </summary>
    Failed,

    /// <summary>
    /// The run was cancelled before it completed.
    /// </summary>
    Canceled,
}
