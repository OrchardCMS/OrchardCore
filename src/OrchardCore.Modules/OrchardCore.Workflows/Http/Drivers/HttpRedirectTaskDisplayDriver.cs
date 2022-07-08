using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Http.Drivers
{
    public class HttpRedirectTaskDisplayDriver : ActivityDisplayDriver<HttpRedirectTask, HttpRedirectTaskViewModel>
    {
        protected override void EditActivity(HttpRedirectTask activity, HttpRedirectTaskViewModel model)
        {
            model.Location = activity.Location.Expression;
            model.Permanent = activity.Permanent;
        }

        protected override void UpdateActivity(HttpRedirectTaskViewModel model, HttpRedirectTask activity)
        {
            activity.Location = new WorkflowExpression<string>(model.Location?.Trim());
            activity.Permanent = model.Permanent;
        }
    }
}
