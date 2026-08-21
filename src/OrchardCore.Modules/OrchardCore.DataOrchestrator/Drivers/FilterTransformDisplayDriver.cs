using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class FilterTransformDisplayDriver : EtlActivityDisplayDriver<FilterTransform, FilterTransformViewModel>
{
    protected override void EditActivity(FilterTransform activity, FilterTransformViewModel model)
    {
        model.Field = activity.Field;
        model.Operator = activity.Operator;
        model.Value = activity.Value;
    }

    protected override void UpdateActivity(FilterTransformViewModel model, FilterTransform activity)
    {
        activity.Field = model.Field;
        activity.Operator = model.Operator;
        activity.Value = model.Value;
    }
}
