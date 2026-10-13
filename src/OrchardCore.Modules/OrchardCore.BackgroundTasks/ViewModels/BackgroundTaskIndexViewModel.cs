using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.BackgroundTasks.ViewModels;

public class BackgroundTaskIndexViewModel
{
    public AdminIndexOptions Options { get; set; }

    [BindNever]
    public IList<BackgroundTaskEntry> Tasks { get; set; }

    [BindNever]
    public dynamic Pager { get; set; }

    /// <summary>
    /// Whether the execution state of the tasks is available, i.e. whether the background service is registered.
    /// </summary>
    [BindNever]
    public bool IsMonitored { get; set; }
}

public class BackgroundTaskEntry
{
    public string Name { get; set; }

    public string Title { get; set; }

    public bool Enable { get; set; }

    public string Description { get; set; }

    public string Schedule { get; set; }

    /// <summary>
    /// The execution state of the task, or <see langword="null"/> if the background service didn't load it yet.
    /// </summary>
    public BackgroundTaskState State { get; set; }
}
