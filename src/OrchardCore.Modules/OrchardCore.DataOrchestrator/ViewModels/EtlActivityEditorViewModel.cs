using OrchardCore.DataOrchestrator.Activities;

namespace OrchardCore.DataOrchestrator.ViewModels;

public class EtlActivityEditorViewModel
{
    public long PipelineId { get; set; }

    public string ActivityId { get; set; }

    public string ActivityName { get; set; }

    public string ReturnUrl { get; set; }

    public IEtlActivity Activity { get; set; }

    public dynamic Editor { get; set; }
}
