using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.ViewModels;

public class EtlPipelineListViewModel
{
    public IList<EtlPipelineDefinition> Pipelines { get; set; } = [];

    public string Search { get; set; }
}
