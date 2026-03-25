namespace OrchardCore.DataOrchestrator.ViewModels;

public class FormatValueTransformViewModel
{
    public string Field { get; set; }

    public string OutputField { get; set; }

    public string FormatType { get; set; }

    public string FormatString { get; set; }

    public string Culture { get; set; }

    public string TimeZoneId { get; set; }
}
