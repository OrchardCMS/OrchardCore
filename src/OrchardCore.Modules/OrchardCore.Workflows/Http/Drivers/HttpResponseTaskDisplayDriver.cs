using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Http.Drivers
{
    public class HttpResponseTaskDisplayDriver : ActivityDisplayDriver<HttpResponseTask, HttpResponseTaskViewModel>
    {
        protected override void EditActivity(HttpResponseTask activity, HttpResponseTaskViewModel model)
        {
            model.HttpStatusCode = activity.HttpStatusCode;
            model.Content = activity.Content.Expression;
            model.ContentType = activity.ContentType.Expression;
            model.Headers = activity.Headers.Expression;
        }

        protected override void UpdateActivity(HttpResponseTaskViewModel model, HttpResponseTask activity)
        {
            activity.HttpStatusCode = model.HttpStatusCode;
            activity.Content = new WorkflowExpression<string>(model.Content);
            activity.ContentType = new WorkflowExpression<string>(model.ContentType?.Trim());
            activity.Headers = new WorkflowExpression<string>(model.Headers?.Trim());
        }
    }
}
