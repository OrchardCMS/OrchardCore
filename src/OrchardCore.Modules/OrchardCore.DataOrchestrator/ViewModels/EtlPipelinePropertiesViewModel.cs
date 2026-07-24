namespace OrchardCore.DataOrchestrator.ViewModels;

public class EtlPipelinePropertiesViewModel
{
    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsEnabled { get; set; }

    public string Schedule { get; set; }
}
