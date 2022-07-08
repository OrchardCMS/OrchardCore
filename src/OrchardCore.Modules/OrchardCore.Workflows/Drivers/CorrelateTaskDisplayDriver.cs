using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class CorrelateTaskDisplayDriver : ActivityDisplayDriver<CorrelateTask, CorrelateTaskViewModel>
    {
        protected override void EditActivity(CorrelateTask activity, CorrelateTaskViewModel model)
        {
            model.Value = activity.Value.Expression;
        }

        protected override void UpdateActivity(CorrelateTaskViewModel model, CorrelateTask activity)
        {
            activity.Value = new WorkflowExpression<string>(model.Value);
        }
    }
}
