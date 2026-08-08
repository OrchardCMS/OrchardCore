using OrchardCore.DataOrchestrator.Models;

namespace OrchardCore.DataOrchestrator.ViewModels;

public class EtlExecutionLogListViewModel
{
    public EtlPipelineDefinition Pipeline { get; set; }

    public IList<EtlExecutionLog> Logs { get; set; } = [];
}
