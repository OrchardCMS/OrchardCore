using System.ComponentModel.DataAnnotations;
using OrchardCore.Admin;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.SharedLinks;

namespace OrchardCore.DataPipelines.ViewModels;

public sealed class DataPipelineIndexViewModel
{
    public string Search { get; set; }

    public bool CanManage { get; set; }

    /// <summary>
    /// Gets or sets the pipelines, rendered by the AdminList shape.
    /// </summary>
    public AdminListContext List { get; set; }
}

/// <summary>
/// A row of the list of the pipelines: the shape of the row is named after it (<c>DataPipelineEntry_SummaryAdmin</c>).
/// </summary>
public sealed class DataPipelineEntry
{
    public DataPipeline Pipeline { get; set; }

    public DataPipelineRun LastRun { get; set; }

    public bool CanManage { get; set; }

    public bool CanRun { get; set; }
}

/// <summary>
/// A row of the run history: the shape of the row is named after it (<c>DataPipelineRunEntry_SummaryAdmin</c>).
/// </summary>
public sealed class DataPipelineRunEntry
{
    public DataPipelineRun Run { get; set; }

    public bool CanRun { get; set; }

    /// <summary>
    /// Gets or sets whether the row links to the run history of its pipeline, on the history of every pipeline.
    /// </summary>
    public bool ShowPipelineHistory { get; set; }
}

public sealed class DataPipelineCreateViewModel
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }
}

public sealed class DataPipelineSettingsViewModel
{
    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsEnabled { get; set; } = true;
}

public sealed class DataPipelineDesignerViewModel
{
    public DataPipeline Pipeline { get; set; }

    public string ConfigJson { get; set; }

    public DataPipelineVersion Version { get; set; }
}

public sealed class DataPipelineRunsViewModel
{
    public DataPipeline Pipeline { get; set; }

    public string Search { get; set; }

    public DataPipelineRunStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the runs, rendered by the AdminList shape.
    /// </summary>
    public AdminListContext List { get; set; }
}

public sealed class DataPipelineRunViewModel
{
    public DataPipelineRun Run { get; set; }

    // The names of the steps of the run, by their identifiers.
    public IReadOnlyDictionary<string, string> StepNames { get; set; } = new Dictionary<string, string>();

    public DataPipeline Pipeline { get; set; }

    public bool CanCancel { get; set; }
}

public sealed class SharedLinksViewModel
{
    public List<SharedLink> Links { get; set; } = [];

    // The display names of the kinds of the listed links.
    public IReadOnlyDictionary<string, string> KindNames { get; set; } = new Dictionary<string, string>();

    // The admin pages about what shared the listed links, by link.
    public IReadOnlyDictionary<string, string> SourceUrls { get; set; } = new Dictionary<string, string>();

    public string Search { get; set; }

    public int TotalCount { get; set; }

    public DateTime UtcNow { get; set; }

    public dynamic Pager { get; set; }
}
