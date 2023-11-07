using OrchardCore.Forms.Activities.ViewModels;
using OrchardCore.Forms.Drivers;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Activities.Drivers;

public class HttpRedirectToFormLocationTaskDisplayDriver : ActivityDisplayDriver<HttpRedirectToFormLocationTask, HttpRedirectToFormLocationTaskViewModel>
{
    protected override void EditActivity(HttpRedirectToFormLocationTask activity, HttpRedirectToFormLocationTaskViewModel model)
    {
        model.FormLocationKey = activity.FormLocationKey.Expression ?? FormPartDisplayDriver.DefaultFormLocationInputName;
    }

    protected override void UpdateActivity(HttpRedirectToFormLocationTaskViewModel model, HttpRedirectToFormLocationTask activity)
    {
        activity.FormLocationKey = new WorkflowExpression<string>(model.FormLocationKey.Trim());
    }
}
