using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Http.Drivers
{
    public class HttpRequestTaskDisplayDriver : ActivityDisplayDriver<HttpRequestTask, HttpRequestTaskViewModel>
    {
        protected override void EditActivity(HttpRequestTask activity, HttpRequestTaskViewModel model)
        {
            model.Url = activity.Url.Expression;
            model.HttpMethod = activity.HttpMethod;
            model.Body = activity.Body.Expression;
            model.ContentType = activity.ContentType.Expression;
            model.Headers = activity.Headers.Expression;
            model.HttpResponseCodes = activity.HttpResponseCodes;
        }

        protected override void UpdateActivity(HttpRequestTaskViewModel model, HttpRequestTask activity)
        {
            activity.Url = new WorkflowExpression<string>(model.Url?.Trim());
            activity.HttpMethod = model.HttpMethod;
            activity.Body = new WorkflowExpression<string>(model.Body);
            activity.ContentType = new WorkflowExpression<string>(model.ContentType?.Trim());
            activity.Headers = new WorkflowExpression<string>(model.Headers?.Trim());
            activity.HttpResponseCodes = model.HttpResponseCodes;
        }
    }
}
