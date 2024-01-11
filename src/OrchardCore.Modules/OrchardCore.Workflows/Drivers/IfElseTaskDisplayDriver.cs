using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class IfElseTaskDisplayDriver : ActivityDisplayDriver<IfElseTask, IfElseTaskViewModel>
    {
        protected override void EditActivity(IfElseTask activity, IfElseTaskViewModel model)
        {
            model.ConditionExpression = activity.Condition.Expression;
        }

        protected override void UpdateActivity(IfElseTaskViewModel model, IfElseTask activity)
        {
            activity.Condition = new WorkflowExpression<bool>(model.ConditionExpression);
        }
    }
}
