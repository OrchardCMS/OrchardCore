using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class JsonSourceDisplayDriver : EtlActivityDisplayDriver<JsonSource, JsonSourceViewModel>
{
    protected override void EditActivity(JsonSource activity, JsonSourceViewModel model)
    {
        model.Data = activity.Data;
    }

    protected override void UpdateActivity(JsonSourceViewModel model, JsonSource activity)
    {
        activity.Data = model.Data;
    }
}
