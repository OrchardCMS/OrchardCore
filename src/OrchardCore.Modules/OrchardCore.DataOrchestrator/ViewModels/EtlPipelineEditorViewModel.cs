using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.ViewModels;

public class EtlPipelineEditorViewModel
{
    public EtlPipelineDefinition Pipeline { get; set; }

    public string PipelineJson { get; set; }

    public IList<dynamic> ActivityThumbnailShapes { get; set; } = [];

    public IList<dynamic> ActivityDesignShapes { get; set; } = [];

    public IList<string> ActivityCategories { get; set; } = [];

    public string LocalId { get; set; }

    public bool LoadLocalState { get; set; }

    public string State { get; set; }
}
