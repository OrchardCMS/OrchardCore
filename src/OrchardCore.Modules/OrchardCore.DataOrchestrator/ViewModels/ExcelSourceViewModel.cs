namespace OrchardCore.DataOrchestrator.ViewModels;

public class ExcelSourceViewModel
{
    public string FilePath { get; set; }

    public string WorksheetName { get; set; }

    public bool HasHeaderRow { get; set; } = true;
}
