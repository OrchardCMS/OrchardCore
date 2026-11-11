using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Admin;
using OrchardCore.Jobs.Models;

namespace OrchardCore.Jobs.ViewModels;

public class JobIndexViewModel
{
    public string Search { get; set; }

    /// <summary>
    /// The data context for rendering the jobs, the toolbar and the pager in the configured layout.
    /// </summary>
    [BindNever]
    public AdminListContext List { get; set; }
}

/// <summary>
/// A registered job, as a row of the jobs admin list.
/// </summary>
public class JobEntry
{
    public string Name { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public bool CanRunManually { get; set; }

    public bool CanManage { get; set; }

    /// <summary>
    /// The most recent run of the job, if any.
    /// </summary>
    public JobRun LastRun { get; set; }

    /// <summary>
    /// The schedules of the job.
    /// </summary>
    public IReadOnlyList<JobSchedule> Schedules { get; set; } = [];
}

public class JobRunNowViewModel
{
    public string Name { get; set; }

    [BindNever]
    public string JobTitle { get; set; }

    [BindNever]
    public string JobDescription { get; set; }

    public string Title { get; set; }

    public string Parameters { get; set; }
}

public class JobScheduleIndexViewModel
{
    /// <summary>
    /// The data context for rendering the schedules and the toolbar in the configured layout.
    /// </summary>
    [BindNever]
    public AdminListContext List { get; set; }

    [BindNever]
    public bool CanCreate { get; set; }
}

public class JobScheduleEditViewModel
{
    public string ScheduleId { get; set; }

    [Required]
    public string JobName { get; set; }

    public string Name { get; set; }

    [Required]
    public string Cron { get; set; }

    public bool IsEnabled { get; set; } = true;

    public string Parameters { get; set; }

    [BindNever]
    public bool IsNew { get; set; }

    [BindNever]
    public bool IsDeclared { get; set; }

    [BindNever]
    public string DefaultCron { get; set; }

    [BindNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Jobs { get; set; } = [];
}

/// <summary>
/// What the shapes of a row of the schedules admin list show.
/// </summary>
public class JobScheduleRowViewModel
{
    public JobSchedule Schedule { get; set; }

    public string JobTitle { get; set; }

    public bool IsJobRegistered { get; set; }

    public bool CanManage { get; set; }

    public bool CanDelete { get; set; }

    /// <summary>
    /// Whether the value is shown in a column of its own, whose header already names it.
    /// </summary>
    public bool InGrid { get; set; }
}
