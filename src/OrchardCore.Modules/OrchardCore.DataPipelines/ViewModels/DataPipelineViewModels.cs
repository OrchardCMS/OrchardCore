using System.ComponentModel.DataAnnotations;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.SharedLinks;

namespace OrchardCore.DataPipelines.ViewModels;

public sealed class DataPipelineIndexViewModel
{
    public List<DataPipelineEntryViewModel> Pipelines { get; set; } = [];

    public string Search { get; set; }

    public int TotalCount { get; set; }

    public dynamic Pager { get; set; }

    public bool CanManage { get; set; }

    public bool CanRun { get; set; }
}

public sealed class DataPipelineEntryViewModel
{
    public DataPipeline Pipeline { get; set; }

    public DataPipelineRun LastRun { get; set; }
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

    public int TotalCount { get; set; }

    public List<DataPipelineRun> Runs { get; set; } = [];

    public dynamic Pager { get; set; }

    public bool CanRun { get; set; }
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
