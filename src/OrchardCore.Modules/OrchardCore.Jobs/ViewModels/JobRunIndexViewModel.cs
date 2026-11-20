using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Admin;
using OrchardCore.Jobs.Models;

namespace OrchardCore.Jobs.ViewModels;

public class JobRunIndexViewModel
{
    public JobRunIndexOptions Options { get; set; } = new();

    /// <summary>
    /// The data context for rendering the runs, the toolbar and the pager in the configured layout.
    /// </summary>
    [BindNever]
    public AdminListContext List { get; set; }
}

public class JobRunIndexOptions
{
    public string Status { get; set; }

    public string JobName { get; set; }

    public JobRunBulkAction BulkAction { get; set; }

    [BindNever]
    public List<SelectListItem> Statuses { get; set; }

    [BindNever]
    public List<SelectListItem> Jobs { get; set; }

    [BindNever]
    public List<SelectListItem> BulkActions { get; set; }
}

public enum JobRunBulkAction
{
    None,
    Cancel,
    Rerun,
    Delete,
}

public class JobRunDetailsViewModel
{
    public JobRun Run { get; set; }

    public string JobTitle { get; set; }

    public bool IsJobRegistered { get; set; }

    public JobRun Parent { get; set; }

    public JobSchedule Schedule { get; set; }

    public IReadOnlyList<JobRun> Children { get; set; } = [];

    public bool CanManage { get; set; }
}

/// <summary>
/// What the shapes of a row of the runs admin list show.
/// </summary>
public class JobRunRowViewModel
{
    public JobRun Run { get; set; }

    public string JobTitle { get; set; }

    public bool IsJobRegistered { get; set; }

    public bool CanManage { get; set; }

    /// <summary>
    /// Whether the value is shown in a column of its own, whose header already names it.
    /// </summary>
    public bool InGrid { get; set; }
}
