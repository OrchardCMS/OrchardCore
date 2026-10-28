namespace OrchardCore.BackgroundTasks;

/// <summary>
/// Describes the execution state of a background task on the current node.
/// </summary>
/// <remarks>
/// The state is kept in memory by the background service of each node, so it reflects the runs
/// of the node serving the request since the application started.
/// </remarks>
public class BackgroundTaskState
{
    /// <summary>
    /// The technical name of the background task.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The name of the tenant the background task belongs to.
    /// </summary>
    public string Tenant { get; set; }

    /// <summary>
    /// The UTC date and time when the last run started, or <see cref="DateTime.MinValue"/> if the task has not run yet.
    /// </summary>
    public DateTime LastStartTime { get; set; }

    /// <summary>
    /// The execution status of the task.
    /// </summary>
    public BackgroundTaskStatus Status { get; set; }

    /// <summary>
    /// The UTC date and time when a run was requested on demand, if <see cref="Status"/> is <see cref="BackgroundTaskStatus.Queued"/>.
    /// </summary>
    public DateTime? QueuedTime { get; set; }

    /// <summary>
    /// The UTC date and time of the next scheduled occurrence, or <see langword="null"/> if the task is disabled
    /// or if its schedule is not a valid cron expression. It can be in the past when the occurrence is due and
    /// waits for the background service to pick it up.
    /// </summary>
    public DateTime? NextStartTime { get; set; }

    /// <summary>
    /// The last completed run, or <see langword="null"/> if the task has not completed any run yet.
    /// </summary>
    public BackgroundTaskRun LastRun { get; set; }

    /// <summary>
    /// The number of completed runs.
    /// </summary>
    public long RunCount { get; set; }

    /// <summary>
    /// The number of completed runs that failed.
    /// </summary>
    public long FailureCount { get; set; }

    /// <summary>
    /// The most recent completed runs, the most recent first.
    /// </summary>
    public IReadOnlyList<BackgroundTaskRun> RecentRuns { get; set; } = [];
}
