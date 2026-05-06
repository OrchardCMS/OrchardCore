namespace OrchardCore.DataOrchestrator.ViewModels;

public class EtlActivityEditorPostModel
{
    public long PipelineId { get; set; }

    public string ActivityId { get; set; }

    public string ActivityName { get; set; }

    public string ReturnUrl { get; set; }
}
