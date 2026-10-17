namespace OrchardCore.BackgroundTasks.ViewModels;

public class BackgroundTaskActionsViewModel
{
    public string Name { get; set; }

    public bool Enable { get; set; }

    /// <summary>
    /// Whether the execution state of the task is available, i.e. whether the task can be run on demand.
    /// </summary>
    public bool IsMonitored { get; set; }

    public BackgroundTaskState State { get; set; }

    /// <summary>
    /// Whether to include a link to the details page of the task.
    /// </summary>
    public bool ShowDetails { get; set; }
}
