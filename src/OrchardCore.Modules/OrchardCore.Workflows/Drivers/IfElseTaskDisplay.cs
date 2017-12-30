using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class IfElseTaskDisplay : ActivityDisplayDriver<IfElseTask, IfElseTaskViewModel>
    {
        protected override void Map(IfElseTask source, IfElseTaskViewModel target)
        {
            target.ConditionExpression = source.ConditionExpression.Expression;
        }

        protected override void Map(IfElseTaskViewModel source, IfElseTask target)
        {
            target.ConditionExpression = new WorkflowExpression<bool>(source.ConditionExpression);
        }
    }
}
