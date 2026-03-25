namespace OrchardCore.DataOrchestrator.ViewModels;

public class FieldMappingTransformViewModel
{
    public string MappingsJson { get; set; }

    public IList<string> AvailableSourceFields { get; set; } = [];
}
