using System.ComponentModel.DataAnnotations;
using OrchardCore.DataPipelines.Models;

namespace OrchardCore.DataPipelines.ViewModels;

public sealed class DataPipelineIndexViewModel
{
    public List<DataPipelineEntryViewModel> Pipelines { get; set; } = [];

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

    public List<DataPipelineRun> Runs { get; set; } = [];

    public dynamic Pager { get; set; }

    public bool CanRun { get; set; }
}

public sealed class DataPipelineRunViewModel
{
    public DataPipelineRun Run { get; set; }

    public DataPipeline Pipeline { get; set; }

    public bool CanCancel { get; set; }
}
