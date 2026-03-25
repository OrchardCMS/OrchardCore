using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class JsonExportLoadDisplayDriver : EtlActivityDisplayDriver<JsonExportLoad, JsonExportLoadViewModel>
{
    protected override void EditActivity(JsonExportLoad activity, JsonExportLoadViewModel model)
    {
        model.FileName = activity.FileName;
    }

    protected override void UpdateActivity(JsonExportLoadViewModel model, JsonExportLoad activity)
    {
        activity.FileName = model.FileName;
    }
}
