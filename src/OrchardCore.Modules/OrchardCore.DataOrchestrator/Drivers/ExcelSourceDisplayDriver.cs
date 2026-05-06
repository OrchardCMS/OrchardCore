using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class ExcelSourceDisplayDriver : EtlActivityDisplayDriver<ExcelSource, ExcelSourceViewModel>
{
    protected override void EditActivity(ExcelSource activity, ExcelSourceViewModel model)
    {
        model.FilePath = activity.FilePath;
        model.WorksheetName = activity.WorksheetName;
        model.HasHeaderRow = activity.HasHeaderRow;
    }

    protected override void UpdateActivity(ExcelSourceViewModel model, ExcelSource activity)
    {
        activity.FilePath = model.FilePath;
        activity.WorksheetName = model.WorksheetName;
        activity.HasHeaderRow = model.HasHeaderRow;
    }
}
