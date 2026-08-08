using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class FormatValueTransformDisplayDriver : EtlActivityDisplayDriver<FormatValueTransform, FormatValueTransformViewModel>
{
    protected override void EditActivity(FormatValueTransform activity, FormatValueTransformViewModel model)
    {
        model.Field = activity.Field;
        model.OutputField = activity.OutputField;
        model.FormatType = activity.FormatType;
        model.FormatString = activity.FormatString;
        model.Culture = activity.Culture;
        model.TimeZoneId = activity.TimeZoneId;
    }

    protected override void UpdateActivity(FormatValueTransformViewModel model, FormatValueTransform activity)
    {
        activity.Field = model.Field;
        activity.OutputField = model.OutputField;
        activity.FormatType = model.FormatType;
        activity.FormatString = model.FormatString;
        activity.Culture = model.Culture;
        activity.TimeZoneId = model.TimeZoneId;
    }
}
