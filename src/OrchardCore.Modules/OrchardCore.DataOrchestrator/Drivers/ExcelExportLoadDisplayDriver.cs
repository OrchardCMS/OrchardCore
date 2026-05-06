using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class ExcelExportLoadDisplayDriver : EtlActivityDisplayDriver<ExcelExportLoad, ExcelExportLoadViewModel>
{
    protected override void EditActivity(ExcelExportLoad activity, ExcelExportLoadViewModel model)
    {
        model.FileName = activity.FileName;
        model.WorksheetName = activity.WorksheetName;
    }

    protected override void UpdateActivity(ExcelExportLoadViewModel model, ExcelExportLoad activity)
    {
        activity.FileName = model.FileName;
        activity.WorksheetName = model.WorksheetName;
    }
}
