using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class WhileLoopTaskDisplay : ActivityDisplayDriver<WhileLoopTask, WhileLoopTaskViewModel>
    {
        protected override void Map(WhileLoopTask source, WhileLoopTaskViewModel target)
        {
            target.ConditionExpression = source.Condition.Expression;
        }

        protected override void Map(WhileLoopTaskViewModel source, WhileLoopTask target)
        {
            target.Condition = new WorkflowExpression<bool>(source.ConditionExpression.Trim());
        }
    }
}
