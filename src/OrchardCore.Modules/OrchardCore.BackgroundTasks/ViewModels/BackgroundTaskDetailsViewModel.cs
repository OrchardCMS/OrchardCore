namespace OrchardCore.BackgroundTasks.ViewModels;

public class BackgroundTaskDetailsViewModel
{
    public string Name { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public bool Enable { get; set; }

    public string Schedule { get; set; }

    public string DefaultSchedule { get; set; }

    public bool UsePipeline { get; set; }

    public int LockTimeout { get; set; }

    public int LockExpiration { get; set; }

    public bool IsAtomic => LockTimeout > 0 && LockExpiration > 0;

    /// <summary>
    /// Whether the execution state of the task is available, i.e. whether the background service is registered.
    /// </summary>
    public bool IsMonitored { get; set; }

    /// <summary>
    /// The execution state of the task, or <see langword="null"/> if the background service didn't load it yet.
    /// </summary>
    public BackgroundTaskState State { get; set; }
}
