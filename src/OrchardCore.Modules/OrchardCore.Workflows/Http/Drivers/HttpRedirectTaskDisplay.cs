using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Http.ViewModels;

namespace OrchardCore.Workflows.Http.Drivers
{
    public class HttpRedirectTaskDisplay : ActivityDisplayDriver<HttpRedirectTask, HttpRedirectTaskViewModel>
    {
        protected override void Map(HttpRedirectTask source, HttpRedirectTaskViewModel target)
        {
            target.Location = source.Location.Expression;
            target.Permanent = source.Permanent.Expression;
        }

        protected override void Map(HttpRedirectTaskViewModel source, HttpRedirectTask target)
        {
            target.Location = new WorkflowExpression<string>(source.Location?.Trim());
            target.Permanent = new WorkflowExpression<bool>(source.Permanent?.Trim());
        }
    }
}
