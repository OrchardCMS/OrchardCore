using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class WhileLoopTaskDisplayDriver : ActivityDisplayDriver<WhileLoopTask, WhileLoopTaskViewModel>
    {
        protected override void EditActivity(WhileLoopTask source, WhileLoopTaskViewModel model)
        {
            model.ConditionExpression = source.Condition.Expression;
        }

        protected override void UpdateActivity(WhileLoopTaskViewModel model, WhileLoopTask activity)
        {
            activity.Condition = new WorkflowExpression<bool>(model.ConditionExpression.Trim());
        }
    }
}
