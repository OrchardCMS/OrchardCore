using OrchardCore.Forms.Activities.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Forms.Activities.Drivers;

public sealed class HttpRedirectToFormLocationTaskDisplayDriver : ActivityDisplayDriver<HttpRedirectToFormLocationTask, HttpRedirectToFormLocationTaskViewModel>
{
    protected override void EditActivity(HttpRedirectToFormLocationTask activity, HttpRedirectToFormLocationTaskViewModel model)
    {
        model.FormLocationKey = activity.FormLocationKey;
    }

    protected override void UpdateActivity(HttpRedirectToFormLocationTaskViewModel model, HttpRedirectToFormLocationTask activity)
    {
        activity.FormLocationKey = model.FormLocationKey?.Trim();
    }
}
