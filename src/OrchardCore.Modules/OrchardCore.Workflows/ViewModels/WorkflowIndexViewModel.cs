using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

public class WorkflowIndexViewModel
{
    public WorkflowIndexViewModel()
    {
        Options = new WorkflowIndexOptions();
    }

    /// <summary>
    /// The workflow type whose instances are listed, or <see langword="null"/> when the instances of every workflow
    /// type are.
    /// </summary>
    public WorkflowType WorkflowType { get; set; }

    public IList<WorkflowEntry> Workflows { get; set; }
    public WorkflowIndexOptions Options { get; set; }
    public dynamic Pager { get; set; }
    public string ReturnUrl { get; set; }

    /// <summary>
    /// The number of instances of each status filter, with the other filters applied.
    /// </summary>
    public IReadOnlyDictionary<WorkflowFilter, int> StatusCounts { get; set; } = new Dictionary<WorkflowFilter, int>();
}

public class WorkflowIndexOptions
{
    public WorkflowIndexOptions()
    {
        Filter = WorkflowFilter.All;
    }

    public WorkflowBulkAction BulkAction { get; set; }
    public WorkflowFilter Filter { get; set; }

    public WorkflowOrder OrderBy { get; set; }

    /// <summary>
    /// The <see cref="WorkflowType.WorkflowTypeId"/> whose instances are listed, on the page of every workflow's
    /// instances.
    /// </summary>
    public string WorkflowTypeId { get; set; }

    /// <summary>
    /// How recently the listed instances were created.
    /// </summary>
    public WorkflowCreatedFilter Created { get; set; }

    [BindNever]
    public List<SelectListItem> WorkflowsSorts { get; set; }

    [BindNever]
    public List<SelectListItem> WorkflowsStatuses { get; set; }

    [BindNever]
    public List<SelectListItem> WorkflowsBulkAction { get; set; }

    [BindNever]
    public List<SelectListItem> WorkflowsCreated { get; set; }

    [BindNever]
    public List<SelectListItem> WorkflowTypes { get; set; }
}

public class WorkflowEntry
{
    public Workflow Workflow { get; set; }
    public long Id { get; set; }
    public bool IsChecked { get; set; }

    /// <summary>
    /// The number of the version the instance runs on, or <see langword="null"/> for instances created before
    /// workflow types had versions.
    /// </summary>
    public int? Version { get; set; }

    /// <summary>
    /// Whether the instance runs on the published version.
    /// </summary>
    public bool IsPublishedVersion { get; set; }

    /// <summary>
    /// The workflow type of the instance, on the page of every workflow's instances.
    /// </summary>
    public WorkflowType WorkflowType { get; set; }
}

/// <summary>
/// The statuses the instances lists filter by.
/// </summary>
public enum WorkflowFilter
{
    All,
    Finished,
    Faulted,

    /// <summary>
    /// Waiting on an event.
    /// </summary>
    Halted,

    /// <summary>
    /// Starting, running or resuming, or idle.
    /// </summary>
    Running,

    /// <summary>
    /// Canceled.
    /// </summary>
    Aborted,
}

/// <summary>
/// How recently the listed instances were created.
/// </summary>
public enum WorkflowCreatedFilter
{
    Any,
    Last24Hours,
    Last7Days,
    Last30Days,
}

public enum WorkflowOrder
{
    CreatedDesc,
    Created,
}

public enum WorkflowBulkAction
{
    None,
    Delete,

    /// <summary>
    /// Runs the faulted instances again from the activity that faulted.
    /// </summary>
    Retry,

    /// <summary>
    /// Aborts the instances that haven't ended: they stop waiting, and their pending retry is dropped.
    /// </summary>
    Cancel,
}
