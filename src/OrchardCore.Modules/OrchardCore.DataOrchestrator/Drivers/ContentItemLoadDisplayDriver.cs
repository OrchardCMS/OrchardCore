using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class ContentItemLoadDisplayDriver : EtlActivityDisplayDriver<ContentItemLoad, ContentItemLoadViewModel>
{
    protected override void EditActivity(ContentItemLoad activity, ContentItemLoadViewModel model)
    {
        model.ContentType = activity.ContentType;
    }

    protected override void UpdateActivity(ContentItemLoadViewModel model, ContentItemLoad activity)
    {
        activity.ContentType = model.ContentType;
    }
}
