using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Http.Drivers
{
    public class HttpRequestTaskDisplay : ActivityDisplayDriver<HttpRequestTask, HttpRequestTaskViewModel>
    {
        protected override void Map(HttpRequestTask source, HttpRequestTaskViewModel target)
        {
            target.Url = source.Url.Expression;
            target.HttpMethod = source.HttpMethod.Expression;
            target.Body = source.Body.Expression;
            target.ContentType = source.ContentType.Expression;
            target.Headers = source.Headers.Expression;
            target.HttpResponseCodes = source.HttpResponseCodes;
        }

        protected override void Map(HttpRequestTaskViewModel source, HttpRequestTask target)
        {
            target.Url = new WorkflowExpression<string>(source.Url?.Trim());
            target.HttpMethod = new WorkflowExpression<string>(source.HttpMethod?.Trim());
            target.Body = new WorkflowExpression<string>(source.Body);
            target.ContentType = new WorkflowExpression<string>(source.ContentType?.Trim());
            target.Headers = new WorkflowExpression<string>(source.Headers?.Trim());
            target.HttpResponseCodes = source.HttpResponseCodes;
        }
    }
}
